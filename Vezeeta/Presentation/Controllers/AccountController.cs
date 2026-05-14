using Application.DTOs.Auth;
using Application.Interfaces.Services.Auth;
using Application.Results;
using Domain.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

public sealed class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IPasswordService _passwordService;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(
        IAuthService authService,
        IPasswordService passwordService,
        SignInManager<ApplicationUser> signInManager)
    {
        _authService = authService;
        _passwordService = passwordService;
        _signInManager = signInManager;
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
        returnUrl ??= "/dashboard";

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