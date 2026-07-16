using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AfterQuake.Domain.Entities;
using AfterQuake.Web.Middleware;
using AfterQuake.Web.Services;
using AfterQuake.Web.Models;

namespace AfterQuake.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly CaptchaService _captchaService;
    private readonly EmailService _emailService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        CaptchaService captchaService,
        EmailService emailService,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _captchaService = captchaService;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        var captcha = _captchaService.Generate();
        ViewBag.CaptchaId = captcha.id;
        ViewBag.CaptchaSvg = captcha.svg;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        if (!_captchaService.Validate(model.CaptchaId ?? "", model.CaptchaCode ?? ""))
        {
            ModelState.AddModelError(string.Empty, "Código captcha incorrecto.");
            var captcha = _captchaService.Generate();
            ViewBag.CaptchaId = captcha.id;
            ViewBag.CaptchaSvg = captcha.svg;
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            _userManager.PasswordHasher.VerifyHashedPassword(null, "$2a$11$K4YfGqJ1e4YHIpRufVY7NO7vW7tJ3c1y2kL3m4n5o6p7q8r9s0t1u2v3w4x5y6z", model.Password);
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            var captcha = _captchaService.Generate();
            ViewBag.CaptchaId = captcha.id;
            ViewBag.CaptchaSvg = captcha.svg;
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Tu cuenta ha sido desactivada.");
            return View(model);
        }

        var ip = HttpContext.Items["ClientIp"] as string ?? "unknown";

        var result = await _signInManager.PasswordSignInAsync(user.UserName ?? user.Email!, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            IpBlockingMiddleware.RecordSuccess(ip);
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return RedirectToLocal(returnUrl);
        }

        IpBlockingMiddleware.RecordFailure(ip);

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Cuenta bloqueada por muchos intentos. Espera 15 minutos.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
        var newCaptcha = _captchaService.Generate();
        ViewBag.CaptchaId = newCaptcha.id;
        ViewBag.CaptchaSvg = newCaptcha.svg;
        return View(model);
    }

    [HttpGet]
    public IActionResult RefreshCaptcha()
    {
        var captcha = _captchaService.Generate();
        return Json(new { id = captcha.id, svg = captcha.svg });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        var captcha = _captchaService.Generate();
        ViewBag.CaptchaId = captcha.id;
        ViewBag.CaptchaSvg = captcha.svg;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (!_captchaService.Validate(model.CaptchaId ?? "", model.CaptchaCode ?? ""))
        {
            ModelState.AddModelError(string.Empty, "Código captcha incorrecto.");
            var captcha = _captchaService.Generate();
            ViewBag.CaptchaId = captcha.id;
            ViewBag.CaptchaSvg = captcha.svg;
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            DocumentId = model.DocumentId,
            EmergencyContact = model.EmergencyContact,
            PreferredLanguage = "es",
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Citizen");
            await _userManager.SetAuthenticationTokenAsync(user, "PasswordExpiration", "LastChanged", DateTime.UtcNow.ToString("O"));

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);
            try
            {
                await _emailService.SendAsync(user.Email!, "Confirma tu cuenta en AfterQuake",
                    $"<h2>Bienvenido a AfterQuake</h2><p>Haz clic <a href='{callbackUrl}'>aquí</a> para confirmar tu correo.</p>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo de confirmación a {Email}", user.Email);
            }

            TempData["SuccessMessage"] = "Cuenta creada exitosamente. Revisa tu correo para confirmar la cuenta antes de iniciar sesión.";
            return RedirectToAction(nameof(Login));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            return RedirectToAction(nameof(HomeController.Index), "Home");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return RedirectToAction(nameof(HomeController.Index), "Home");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return View(result.Succeeded ? "ConfirmEmail" : "Error");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Account", new { email = model.Email, token }, Request.Scheme);
            try
            {
                await _emailService.SendAsync(user.Email!, "Recuperación de contraseña - AfterQuake",
                    $"<h2>Recupera tu contraseña</h2><p>Haz clic <a href='{resetLink}'>aquí</a> para restablecer tu contraseña.</p>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo de recuperación a {Email}", user.Email);
            }
        }

        ViewBag.Message = "Si el correo existe, recibirás un enlace de recuperación.";
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? email = null, string? token = null)
    {
        var model = new ResetPasswordViewModel { Email = email ?? "", Token = token ?? "" };
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Usuario no encontrado.");
            return View(model);
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (result.Succeeded)
        {
            ViewBag.Message = "Contraseña restablecida exitosamente.";
            return View(model);
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        var twoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

        var model = new ProfileViewModel
        {
            FullName = user.FullName ?? "",
            Email = user.Email ?? "",
            DocumentId = user.DocumentId,
            EmergencyContact = user.EmergencyContact,
            PreferredLanguage = user.PreferredLanguage ?? "es",
            TwoFactorEnabled = twoFactorEnabled,
            PasswordExpired = HttpContext.Request.Query["expired"] == "true"
        };
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        user.FullName = model.FullName;
        user.DocumentId = model.DocumentId;
        user.EmergencyContact = model.EmergencyContact;
        user.PreferredLanguage = model.PreferredLanguage;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Perfil actualizado exitosamente.";
            return RedirectToAction(nameof(Profile));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
    {
        if (newPassword != confirmNewPassword)
        {
            TempData["ErrorMessage"] = "Las contraseñas no coinciden.";
            return RedirectToAction(nameof(Profile), new { expired = true });
        }
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 12)
        {
            TempData["ErrorMessage"] = "La nueva contraseña debe tener al menos 12 caracteres.";
            return RedirectToAction(nameof(Profile), new { expired = true });
        }
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded)
        {
            await _userManager.SetAuthenticationTokenAsync(user, "PasswordExpiration", "LastChanged", DateTime.UtcNow.ToString("O"));
            TempData["SuccessMessage"] = "Contraseña cambiada exitosamente.";
            return RedirectToAction(nameof(Profile));
        }
        var errors = string.Join(" ", result.Errors.Select(e => e.Description));
        TempData["ErrorMessage"] = errors;
        return RedirectToAction(nameof(Profile), new { expired = true });
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
}

public class LoginViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public string? CaptchaId { get; set; }
    public string? CaptchaCode { get; set; }
}

public class RegisterViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? DocumentId { get; set; }
    public string? EmergencyContact { get; set; }
    public string? CaptchaId { get; set; }
    public string? CaptchaCode { get; set; }
}

public class ForgotPasswordViewModel
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}



