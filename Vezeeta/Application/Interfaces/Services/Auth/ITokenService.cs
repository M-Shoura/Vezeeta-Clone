using Domain.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services.Auth
{
    public interface ITokenService
    {
        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    }
}
