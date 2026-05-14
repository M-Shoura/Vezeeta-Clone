using Application.DTOs.Auth;
using Application.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services.Auth
{
    public interface IPasswordService
    {
        Task<Result> ForgotPasswordAsync(string email);
        Task<Result> ResetPasswordAsync(ResetPasswordDto dto);
        Task<Result> ChangePasswordAsync(ChangePasswordDto dto);
    }
}
