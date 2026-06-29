using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;

namespace AfterQuake.Web.Controllers;

public class HelpRequestController : Controller
{
    private readonly IHelpRequestService _helpRequestService;

    public HelpRequestController(IHelpRequestService helpRequestService) => _helpRequestService = helpRequestService;

    public async Task<IActionResult> Index()
    {
        var requests = await _helpRequestService.GetAllAsync();
        return View(requests);
    }

    [AllowAnonymous]
    public IActionResult Create()
    {
        return View(new CreateHelpRequestDto());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateHelpRequestDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var userId = User.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;
        await _helpRequestService.CreateAsync(dto, userId);
        TempData["SuccessMessage"] = "Solicitud registrada. Pronto alguien te contactará.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(Guid id, Guid volunteerId)
    {
        await _helpRequestService.AssignAsync(id, volunteerId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(Guid id, string? notes)
    {
        await _helpRequestService.ResolveAsync(id, notes);
        return RedirectToAction(nameof(Index));
    }
}
