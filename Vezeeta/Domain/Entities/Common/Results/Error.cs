using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities.Common.Results
{

    public sealed record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);

        // ── Auth ──────────────────────────────────────────
        public static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "Email or password is incorrect.");
        public static readonly Error EmailNotConfirmed = new("Auth.EmailNotConfirmed", "Please confirm your email before logging in.");
        public static readonly Error EmailAlreadyExists = new("Auth.EmailAlreadyExists", "An account with this email already exists.");
        public static readonly Error UserNotFound = new("Auth.UserNotFound", "No account found with this email.");
        public static readonly Error InvalidToken = new("Auth.InvalidToken", "The token is invalid or has expired.");
        public static readonly Error ExternalLoginFailed = new("Auth.ExternalLoginFailed", "External login failed. Please try again.");
        public static readonly Error AccountLocked = new("Auth.AccountLocked", "Your account has been locked. Try again later.");

        // ── General ───────────────────────────────────────
        public static readonly Error Unexpected = new("General.Unexpected", "An unexpected error occurred.");
    }
}
