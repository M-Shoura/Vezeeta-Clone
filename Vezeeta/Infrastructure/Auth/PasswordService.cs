
using Application.DTOs.Auth;
using Application.Interfaces.Services.Auth;
using Application.Results;
using Domain.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Infrastructure.Services.Auth;

public sealed class PasswordService : IPasswordService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PasswordService> _logger;

    public PasswordService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IEmailService emailService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PasswordService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }


    public async Task<Result> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || !user.EmailConfirmed)
            return Result.Success();

        var token = await _tokenService.GeneratePasswordResetTokenAsync(user);
        await _emailService.SendPasswordResetEmailAsync(email, token);

        _logger.LogInformation("Password reset link sent to {Email}", email);
        return Result.Success();
    }


    public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null)
            return Result.Success();

        var decodedToken = Encoding.UTF8.GetString(
            WebEncoders.Base64UrlDecode(dto.Token));

        var result = await _userManager.ResetPasswordAsync(
            user, decodedToken, dto.NewPassword);

        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            return Result.Failure(new Error("Identity.Error", message));
        }

        await _userManager.SetLockoutEndDateAsync(user, null);

        _logger.LogInformation("Password reset for {Email}", dto.Email);
        return Result.Success();
    }


    public async Task<Result> ChangePasswordAsync(ChangePasswordDto dto)
    {
        var claimsPrincipal = _httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException("No authenticated user in context.");

        var user = await _userManager.GetUserAsync(claimsPrincipal);
        if (user is null)
            return Result.Failure(IdentityErrors.UserNotFound);

        var result = await _userManager.ChangePasswordAsync(
            user, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            return Result.Failure(new Error("Identity.Error", message));
        }

        _logger.LogInformation("Password changed for user {UserId}", user.Id);
        return Result.Success();
    }
}