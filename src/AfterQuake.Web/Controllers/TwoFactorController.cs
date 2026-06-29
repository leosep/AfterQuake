using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AfterQuake.Domain.Entities;

namespace AfterQuake.Web.Controllers;

[Authorize]
public class TwoFactorController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public TwoFactorController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Setup()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            key = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        ViewBag.SharedKey = key;
        ViewBag.AuthenticatorUri = $"otpauth://totp/AfterQuake:{user.Email}?secret={key}&issuer=AfterQuake&digits=6";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Setup(string code)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);
        if (!isValid)
        {
            ModelState.AddModelError("", "Código inválido. Intenta nuevamente.");
            return View();
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        TempData["SuccessMessage"] = "Autenticación de dos factores activada exitosamente.";
        return RedirectToAction("Profile", "Account");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        await _userManager.SetTwoFactorEnabledAsync(user, false);
        TempData["SuccessMessage"] = "Autenticación de dos factores desactivada.";
        return RedirectToAction("Profile", "Account");
    }
}
