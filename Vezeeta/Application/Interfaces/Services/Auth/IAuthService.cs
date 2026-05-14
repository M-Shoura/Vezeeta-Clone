using Application.DTOs.Auth;
using Application.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services.Auth
{
    public interface IAuthService
    {
        Task<Result<string>> RegisterAsync(RegisterDto dto);
        Task<Result<string>> LoginAsync(LoginDto dto);
        Task<Result<string>> ExternalLoginAsync(string provider, string returnUrl);
        Task<Result<string>> ConfirmEmailAsync(string userId, string token);
        Task LogoutAsync();
    }
}
