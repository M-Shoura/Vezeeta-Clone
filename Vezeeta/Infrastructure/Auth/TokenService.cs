using Application.Interfaces.Services.Auth;
using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Infrastructure.Services.Auth;

public sealed class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public TokenService(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
    {
        var rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
    }

    public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
    {
        var rawToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
    }
}