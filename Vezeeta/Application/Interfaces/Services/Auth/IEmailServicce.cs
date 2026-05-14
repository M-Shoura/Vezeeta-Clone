using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services.Auth
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string email, string userId, string token);
        Task SendPasswordResetEmailAsync(string email, string token);
    }
}
