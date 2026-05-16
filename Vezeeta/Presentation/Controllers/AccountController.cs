using Application.DTOs.Auth;
using Application.Interfaces.Services.Auth;
using Application.Results;
using Domain.Entities;
using Domain.Enums;
using Domain.Identity;
using Infranstructure.Persistence.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Presentation.ViewModels.Accounts;

namespace Presentation.Controllers;

public sealed class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IPasswordService _passwordService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AccountController(
        IAuthService authService,
        IPasswordService passwordService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IWebHostEnvironment env)
    {
        _authService = authService;
        _passwordService = passwordService;
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
        _env = env;
    }


    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]

    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var result = await _authService.RegisterAsync(dto);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(dto);
        }

        return Redirect(result.Value);    
    }

 

    [HttpGet]
    [AllowAnonymous]

    public IActionResult RegistrationPending() => View();


    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]

    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var result = await _authService.LoginAsync(dto);

        if (result.IsFailure && result.Error == IdentityErrors.AccountUnderReview)
            return RedirectToAction(nameof(UnderReview));

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(dto);
        }

        return LocalRedirect(result.Value);
    }
    [HttpGet]
    [AllowAnonymous]

    public IActionResult UnderReview() => View();

    [HttpGet]
    [Authorize]
    // [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction(nameof(Login));
    }


    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return BadRequest("Invalid email confirmation link.");

        var result = await _authService.ConfirmEmailAsync(userId, token);

        if (result.IsFailure)
        {
            ViewData["Error"] = result.Error.Message;
            return View("ConfirmEmailFailed");
        }

        return Redirect(result.Value);   
    }


    [HttpGet]
    [AllowAnonymous]


    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]

    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        await _passwordService.ForgotPasswordAsync(dto.Email);

        return View("ForgotPasswordConfirmation");
    }


    [HttpGet]
    [AllowAnonymous]

    public IActionResult ResetPassword(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return BadRequest("Invalid password reset link.");

        return View(new ResetPasswordDto { Email = email, Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]

    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var result = await _passwordService.ResetPasswordAsync(dto);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(dto);
        }

        TempData["Success"] = "Password reset successfully. You can now log in.";
        return RedirectToAction(nameof(Login));
    }


    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword() => View();

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var result = await _passwordService.ChangePasswordAsync(dto);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(dto);
        }

        TempData["Success"] = "Password changed successfully.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        var patient = await _context.PatientProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ApplicationUserId == user.Id);

        var doctor = await _context.DoctorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ApplicationUserId == user.Id);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.Contains(UserRole.Doctor.ToString())
            ? UserRole.Doctor
            : UserRole.Patient;

        return View(new ProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            ProfilePicture = user.ProfilePicture,
            Role = role,
            BirthDate = patient?.DateOfBirth,
            Gender = patient?.Gender,
            BloodType = patient?.BloodType,
            EmergencyContactName = patient?.EmergencyContactName,
            EmergencyContactPhone = patient?.EmergencyContactPhone,
            Specialization = doctor?.Specialization,
            Qualification = doctor?.Qualification,
            YearsOfExperience = doctor?.YearsOfExperience,
            Bio = doctor?.Bio,
            LicenseNumber = doctor?.LicenseNumber,
            IsAvailable = doctor?.IsAvailable ?? true
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        if (!User.IsInRole("Doctor") && !User.IsInRole("Patient"))
        {
            ModelState.AddModelError(nameof(model.Role), "Only doctors and patients can change profile type.");
            return View(model);
        }

        if (model.Role != UserRole.Doctor && model.Role != UserRole.Patient)
        {
            ModelState.AddModelError(nameof(model.Role), "Profile type must be Doctor or Patient.");
            return View(model);
        }

        var duplicateEmailUser = await _userManager.FindByEmailAsync(model.Email);
        if (duplicateEmailUser != null && duplicateEmailUser.Id != user.Id)
        {
            ModelState.AddModelError(nameof(model.Email), "Email is already used by another account.");
            return View(model);
        }

        if (model.ProfilePictureFile != null)
        {
            try
            {
                user.ProfilePicture = await SaveProfilePictureAsync(model.ProfilePictureFile);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.ProfilePictureFile), ex.Message);
                return View(model);
            }
        }

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.Address = model.Address;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        await UpdateUserRoleAsync(user, model.Role);
        await UpdateRoleProfileAsync(user.Id, model);
        await _signInManager.RefreshSignInAsync(user);

        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

    private async Task UpdateUserRoleAsync(ApplicationUser user, UserRole role)
    {
        var currentRoles = await _userManager.GetRolesAsync(user);
        var typeRoles = currentRoles
            .Where(r => r == UserRole.Doctor.ToString() || r == UserRole.Patient.ToString())
            .ToArray();

        if (typeRoles.Length > 0)
            await _userManager.RemoveFromRolesAsync(user, typeRoles);

        await _userManager.AddToRoleAsync(user, role.ToString());
    }

    private async Task UpdateRoleProfileAsync(string userId, ProfileViewModel model)
    {
        var patient = await _context.PatientProfiles
            .FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

        var doctor = await _context.DoctorProfiles
            .FirstOrDefaultAsync(d => d.ApplicationUserId == userId);

        if (model.Role == UserRole.Patient)
        {
            patient ??= new PatientProfile { ApplicationUserId = userId };

            patient.DateOfBirth = model.BirthDate ?? DateTime.Today;
            patient.Gender = model.Gender ?? Gender.Male;
            patient.BloodType = model.BloodType;
            patient.EmergencyContactName = model.EmergencyContactName ?? string.Empty;
            patient.EmergencyContactPhone = model.EmergencyContactPhone ?? string.Empty;

            if (_context.Entry(patient).State == EntityState.Detached)
                _context.PatientProfiles.Add(patient);
        }

        if (model.Role == UserRole.Doctor)
        {
            doctor ??= new DoctorProfile { ApplicationUserId = userId };

            doctor.Specialization = model.Specialization ?? string.Empty;
            doctor.Qualification = model.Qualification ?? string.Empty;
            doctor.YearsOfExperience = model.YearsOfExperience ?? 0;
            doctor.Bio = model.Bio;
            doctor.LicenseNumber = model.LicenseNumber;
            doctor.IsAvailable = model.IsAvailable;

            if (_context.Entry(doctor).State == EntityState.Detached)
                _context.DoctorProfiles.Add(doctor);
        }

        await _context.SaveChangesAsync();
    }

    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxImageSizeBytes = 2 * 1024 * 1024;

    private async Task<string> SaveProfilePictureAsync(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedImageExtensions.Contains(extension))
            throw new InvalidOperationException("Only JPG, PNG, and WEBP images are allowed.");

        if (file.Length > MaxImageSizeBytes)
            throw new InvalidOperationException("Profile picture size must not exceed 2MB.");

        var folder = Path.Combine(_env.WebRootPath, "images");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(folder, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return fileName;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var patient = await _context.PatientProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ApplicationUserId == id);

        var doctor = await _context.DoctorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ApplicationUserId == id);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.Contains(UserRole.Doctor.ToString())
            ? UserRole.Doctor
            : UserRole.Patient;

        return View("Profile", new ProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            ProfilePicture = user.ProfilePicture,
            Role = role,
            BirthDate = patient?.DateOfBirth,
            Gender = patient?.Gender,
            BloodType = patient?.BloodType,
            EmergencyContactName = patient?.EmergencyContactName,
            EmergencyContactPhone = patient?.EmergencyContactPhone,
            Specialization = doctor?.Specialization,
            Qualification = doctor?.Qualification,
            YearsOfExperience = doctor?.YearsOfExperience,
            Bio = doctor?.Bio,
            LicenseNumber = doctor?.LicenseNumber,
            IsAvailable = doctor?.IsAvailable ?? true,
            EditingUserId = id
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(string id, ProfileViewModel model)
    {
        model.EditingUserId = id;

        if (!ModelState.IsValid)
            return View("Profile", model);

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var duplicateEmailUser = await _userManager.FindByEmailAsync(model.Email);
        if (duplicateEmailUser != null && duplicateEmailUser.Id != user.Id)
        {
            ModelState.AddModelError(nameof(model.Email), "Email is already used by another account.");
            return View("Profile", model);
        }

        if (model.ProfilePictureFile != null)
        {
            try
            {
                user.ProfilePicture = await SaveProfilePictureAsync(model.ProfilePictureFile);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.ProfilePictureFile), ex.Message);
                return View("Profile", model);
            }
        }

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.Address = model.Address;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View("Profile", model);
        }

        await UpdateRoleProfileAsync(user.Id, model);

        TempData["Success"] = $"Profile for {user.FullName} updated successfully.";
        return RedirectToAction("Index", "Doctor");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(
            nameof(ExternalLoginCallback), "Account", new { returnUrl });

        var properties = _signInManager
            .ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        return Challenge(properties, provider);
    }

    [HttpGet]
    [AllowAnonymous]

    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null)
    {
        returnUrl ??= Url.Action("Index", "Home") ?? "/";

        var result = await _authService.ExternalLoginAsync("Google", returnUrl);

        if (result.IsFailure && result.Error == IdentityErrors.AccountUnderReview)
            return RedirectToAction(nameof(UnderReview));

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Message;
            return RedirectToAction(nameof(Login));
        }

        return LocalRedirect(result.Value);
    }
}
