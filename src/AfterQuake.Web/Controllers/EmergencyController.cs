using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;

namespace AfterQuake.Web.Controllers;

[Authorize]
public class EmergencyController : Controller
{
    private readonly IEmergencyService _emergencyService;

    public EmergencyController(IEmergencyService emergencyService) => _emergencyService = emergencyService;

    public async Task<IActionResult> Index()
    {
        var emergencies = await _emergencyService.GetActiveAsync();
        return View(emergencies);
    }

    [AllowAnonymous]
    public IActionResult Report()
    {
        return View(new CreateEmergencyReportDto());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Report(CreateEmergencyReportDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var userId = User.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;
        await _emergencyService.CreateAsync(dto, userId);
        TempData["SuccessMessage"] = "Reporte de emergencia enviado. Permanece en un lugar seguro.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(Guid id, string? notes)
    {
        await _emergencyService.ResolveAsync(id, notes);
        return RedirectToAction(nameof(Index));
    }
}
