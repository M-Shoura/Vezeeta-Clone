using Application.DTOs.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Auth;
using Application.Results;
using Domain.Entities;
using Domain.Enums;
using Domain.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AuthService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        ITokenService tokenService,
        IEmailService emailService,
        IWebHostEnvironment env,
        ILogger<AuthService> logger,IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        this._roleManager = roleManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _env = env;
        _logger = logger;
        this._unitOfWork = unitOfWork;
    }


    public async Task<Result<string>> RegisterAsync(RegisterDto dto)
    {
        await SeedRolesAsync();
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return Result.Failure<string>(IdentityErrors.EmailAlreadyExists);

        string? picturePath = dto.ProfilePicture is not null
            ? await SaveProfilePictureAsync(dto.ProfilePicture)
            : null;

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            FullName = dto.FullName,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            ProfilePicture = picturePath,

            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            return IdentityFailure<string>(createResult);

        await _userManager.AddToRoleAsync(user, dto.Role.ToString());

        if (dto.Role == UserRole.Doctor)
        {
            var DoctorProfile = new DoctorProfile
            {
                ApplicationUserId = user.Id,
                Specialization = dto.Specialization!,
                Qualification = dto.Qualification!,
                YearsOfExperience = dto.YearsOfExperience ?? 0,
                Bio = dto.Bio,
                LicenseNumber = dto.LicenseNumber,
            };
            await _unitOfWork.Repository<DoctorProfile>().AddAsync(DoctorProfile);
            _unitOfWork.SaveChanges();

        }
        else if (dto.Role == UserRole.Patient)
        {
            var PatientProfile = new PatientProfile
            {
                ApplicationUserId = user.Id,
                DateOfBirth = dto.DateOfBirth!.Value,
                Gender = dto.Gender!.Value,
                BloodType = dto.BloodType,
                EmergencyContactName = dto.EmergencyContactName!,
                EmergencyContactPhone = dto.EmergencyContactPhone!,
            };
            await _unitOfWork.Repository<PatientProfile>().AddAsync(PatientProfile);
            _unitOfWork.SaveChanges();


        }


        var token = await _tokenService.GenerateEmailConfirmationTokenAsync(user);
        await _emailService.SendConfirmationEmailAsync(user.Email!, user.Id, token);

        _logger.LogInformation("New {Role} registered: {Email}", dto.Role, dto.Email);
        return Result.Success<string>("/account/RegistrationPending");
    }


    public async Task<Result<string>> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null || user.IsDeleted)
            return Result.Failure<string>(IdentityErrors.InvalidCredentials);

        //if (!user.EmailConfirmed)
        //    return Result.Failure<string>(IdentityErrors.EmailNotConfirmed);

        if (!user.IsActive)
            return Result.Failure<string>(IdentityErrors.AccountUnderReview);

        var result = await _signInManager.PasswordSignInAsync(
            user, dto.Password, dto.RememberMe, lockoutOnFailure: true);

        if (result.IsLockedOut) return Result.Failure<string>(IdentityErrors.AccountLocked);
        if (!result.Succeeded) return Result.Failure<string>(IdentityErrors.InvalidCredentials);

        _logger.LogInformation("User logged in: {Email}", dto.Email);

        var redirectUrl = string.IsNullOrWhiteSpace(dto.ReturnUrl)
            ? await GetDefaultRedirectAsync(user)
            : dto.ReturnUrl;

        return Result.Success<string>(redirectUrl);
    }


    public async Task<Result<string>> ExternalLoginAsync(string provider, string returnUrl)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
            return Result.Failure<string>(IdentityErrors.ExternalLoginFailed);

        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey,
            isPersistent: false, bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            var existingUser = await _userManager.FindByLoginAsync(
                info.LoginProvider, info.ProviderKey);

            if (existingUser is not null && !existingUser.IsActive)
            {
                await _signInManager.SignOutAsync();
                return Result.Failure<string>(IdentityErrors.AccountUnderReview);
            }

            _logger.LogInformation("User signed in via {Provider}", provider);

            var redirect = existingUser is not null
                ? await GetDefaultRedirectAsync(existingUser)
                : returnUrl;

            return Result.Success<string>(redirect);
        }

        if (signInResult.IsLockedOut)
            return Result.Failure<string>(IdentityErrors.AccountLocked);

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var fullName = info.Principal.FindFirstValue(ClaimTypes.Name);
        var picture = info.Principal.FindFirstValue("picture");

        if (string.IsNullOrEmpty(email))
            return Result.Failure<string>(IdentityErrors.ExternalLoginFailed);

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName ?? email,
                EmailConfirmed = true,
                ProfilePicture = picture,
                IsActive = true,
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return IdentityFailure<string>(createResult);

            await _userManager.AddToRoleAsync(user, UserRole.Patient.ToString());
            _logger.LogInformation("New user created via {Provider}: {Email}", provider, email);
        }

        await _userManager.AddLoginAsync(user, info);

        if (!user.IsActive)
            return Result.Failure<string>(IdentityErrors.AccountUnderReview);

        await _signInManager.SignInAsync(user, isPersistent: false);

        return Result.Success<string>(await GetDefaultRedirectAsync(user));
    }


    public async Task<Result<string>> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure<string>(IdentityErrors.UserNotFound);

        if (user.EmailConfirmed)
        {
            var alreadyConfirmedUrl = !user.IsActive
                ? "/account/underreview"
                : "/account/login?emailConfirmed=true";
            return Result.Success<string>(alreadyConfirmedUrl);
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
            return Result.Failure<string>(IdentityErrors.InvalidToken);

        _logger.LogInformation("Email confirmed for user {UserId}", userId);

        var redirectUrl = !user.IsActive
            ? "/account/underreview"
            : "/account/login?emailConfirmed=true";

        return Result.Success<string>(redirectUrl);
    }


    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User signed out");
    }


    private async Task<string> GetDefaultRedirectAsync(ApplicationUser user)
    {
        if (await _userManager.IsInRoleAsync(user, UserRole.Admin.ToString()))
            return "/Admin/Index";

        if (await _userManager.IsInRoleAsync(user, UserRole.Doctor.ToString()))
            return "/Doctor/Details/"+user.Id;

        if (await _userManager.IsInRoleAsync(user, UserRole.Patient.ToString()))
            return "/Doctor/Index";

        return "/";  
    }

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 2 * 1024 * 1024;   

    private async Task<string> SaveProfilePictureAsync(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException($"File type '{extension}' is not allowed.");

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("File size must not exceed 2MB.");

        var folder = Path.Combine(_env.WebRootPath, "uploads", "profiles");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(folder, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/profiles/{fileName}";
    }
    private static Result<T> IdentityFailure<T>(IdentityResult result)
    {
        var message = string.Join(" ", result.Errors.Select(e => e.Description));
        return Result.Failure<T>(new Error("Identity.Error", message));
    }

    private async Task SeedRolesAsync()
    {
        if (!_roleManager.Roles.Any())
        {
            await _roleManager.CreateAsync(new ApplicationRole() { Name = UserRole.Admin.ToString() });
            await _roleManager.CreateAsync(new ApplicationRole() { Name = UserRole.Patient.ToString() });
            await _roleManager.CreateAsync(new ApplicationRole() { Name = UserRole.Doctor.ToString() });

        }

    }
}