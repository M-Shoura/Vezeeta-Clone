using Application.Interfaces.Services.Auth;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Infrastructure.Services.Auth;

public sealed class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    private string Host => _config["Email:Host"]!;
    private int Port => int.Parse(_config["Email:Port"]!);
    private string From => _config["Email:From"]!;
    private string Username => _config["Email:Username"]!;
    private string Password => _config["Email:Password"]!;
    private string BaseUrl => _config["App:BaseUrl"]!;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendConfirmationEmailAsync(string email, string userId, string token)
    {
        var link = $"{BaseUrl}/account/confirmemail?userId={userId}&token={Uri.EscapeDataString(token)}";

        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;">
                <h2 style="color:#4F46E5;">Confirm Your Email</h2>
                <p>Thank you for registering! Click below to verify your email address.</p>
                <a href="{link}"
                   style="display:inline-block;padding:12px 28px;background:#4F46E5;
                          color:white;border-radius:6px;text-decoration:none;font-weight:bold;">
                    Confirm Email
                </a>
                <p style="margin-top:16px;color:#6B7280;font-size:13px;">
                    This link expires in <strong>24 hours</strong>.
                    If you didn't register, you can safely ignore this email.
                </p>
            </div>
            """;

        await SendAsync(email, "Confirm Your Email", html);
    }

    public async Task SendPasswordResetEmailAsync(string email, string token)
    {
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedToken = Uri.EscapeDataString(token);
        var link = $"{BaseUrl}/account/reset-password?email={encodedEmail}&token={encodedToken}";

        var html = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;">
                <h2 style="color:#4F46E5;">Reset Your Password</h2>
                <p>We received a request to reset your password. Click below to proceed.</p>
                <a href="{link}"
                   style="display:inline-block;padding:12px 28px;background:#4F46E5;
                          color:white;border-radius:6px;text-decoration:none;font-weight:bold;">
                    Reset Password
                </a>
                <p style="margin-top:16px;color:#6B7280;font-size:13px;">
                    This link expires in <strong>1 hour</strong>.
                    If you didn't request this, ignore this email — your password won't change.
                </p>
            </div>
            """;

        await SendAsync(email, "Reset Your Password", html);
    }


    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(From));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            //using var smtp = new SmtpClient();
            //await smtp.ConnectAsync(Host, Port, SecureSocketOptions.StartTls);
            //await smtp.AuthenticateAsync(Username, Password);
            //await smtp.SendAsync(message);
            //await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;   // Let caller decide whether to swallow or bubble
        }
    }
}