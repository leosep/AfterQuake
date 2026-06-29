using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;

namespace AfterQuake.Web.Controllers;

public class ShelterController : Controller
{
    private readonly IShelterService _shelterService;

    public ShelterController(IShelterService shelterService) => _shelterService = shelterService;

    public async Task<IActionResult> Index()
    {
        var shelters = await _shelterService.GetAllAsync();
        return View(shelters);
    }

    public async Task<IActionResult> Map()
    {
        var shelters = await _shelterService.GetActiveAsync();
        return View(shelters);
    }

    [Authorize(Policy = "RequireAdmin")]
    public IActionResult Create()
    {
        return View(new CreateShelterDto());
    }

    [Authorize(Policy = "RequireAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateShelterDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        await _shelterService.CreateAsync(dto, userId);
        TempData["SuccessMessage"] = "Albergue registrado exitosamente.";
        return RedirectToAction(nameof(Index));
    }
}
