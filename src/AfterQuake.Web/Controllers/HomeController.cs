using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Enumerations;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Web.Controllers;

public class HomeController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IEmergencyService _emergencyService;
    private readonly IAlertService _alertService;
    private readonly IUnitOfWork _uow;

    public HomeController(IDashboardService dashboardService, IEmergencyService emergencyService, IAlertService alertService, IUnitOfWork uow)
    {
        _dashboardService = dashboardService;
        _emergencyService = emergencyService;
        _alertService = alertService;
        _uow = uow;
    }

    public async Task<IActionResult> Index()
    {
        var dashboard = await _dashboardService.GetDashboardAsync();
        return View(dashboard);
    }

    [HttpGet]
    public IActionResult EmergencyNumbers()
    {
        return PartialView("_EmergencyNumbers");
    }

    public async Task<IActionResult> Communications()
    {
        var repo = _uow.Repository<OfficialCommunication>();
        var items = await repo.FindAsync(c => c.Status == CommunicationStatus.Published);
        return View(items.OrderByDescending(c => c.PublishedAt).ToList());
    }

    public IActionResult Privacy() => View();

    public IActionResult Terms() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sos(CreateEmergencyReportDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _emergencyService.CreateAsync(dto);
        TempData["SuccessMessage"] = "Emergencia reportada exitosamente. La ayuda está en camino.";
        return RedirectToAction(nameof(Index));
    }
}
