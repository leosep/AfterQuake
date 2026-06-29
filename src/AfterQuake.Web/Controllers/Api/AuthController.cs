using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AfterQuake.Domain.Entities;
using AfterQuake.Web.Services;

namespace AfterQuake.Web.Controllers.Api;

[ApiController]
[EnableRateLimiting("StrictRateLimit")]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtService _jwtService;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, JwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
            return Unauthorized(new { error = "Credenciales inválidas" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
        if (result.IsLockedOut)
            return Unauthorized(new { error = "Cuenta bloqueada" });
        if (!result.Succeeded)
            return Unauthorized(new { error = "Credenciales inválidas" });

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user.Id, user.Email!, roles.FirstOrDefault() ?? "Citizen");

        return Ok(new { token, user.Email, user.FullName, role = roles.FirstOrDefault() });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            IsActive = true
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await _userManager.AddToRoleAsync(user, "Citizen");
        return Ok(new { message = "Usuario registrado exitosamente" });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RegisterRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string FullName { get; set; } = "";
}
