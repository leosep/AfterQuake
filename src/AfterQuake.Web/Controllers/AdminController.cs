using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AfterQuake.Application.Interfaces;
using AfterQuake.Application.DTOs;
using AfterQuake.Domain.Enumerations;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Interfaces;
using AfterQuake.Web.Services;

namespace AfterQuake.Web.Controllers;

[Authorize(Policy = "RequireAdmin")]
[Area("Admin")]
public class AdminController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IEmergencyService _emergencyService;
    private readonly IPersonReportService _personReportService;
    private readonly IHelpRequestService _helpRequestService;
    private readonly IShelterService _shelterService;
    private readonly IAlertService _alertService;
    private readonly IUnitOfWork _uow;
    private readonly ExportService _exportService;

    public AdminController(
        IDashboardService dashboardService,
        IEmergencyService emergencyService,
        IPersonReportService personReportService,
        IHelpRequestService helpRequestService,
        IShelterService shelterService,
        IAlertService alertService,
        IUnitOfWork uow,
        ExportService exportService)
    {
        _dashboardService = dashboardService;
        _emergencyService = emergencyService;
        _personReportService = personReportService;
        _helpRequestService = helpRequestService;
        _shelterService = shelterService;
        _alertService = alertService;
        _uow = uow;
        _exportService = exportService;
    }

    public async Task<IActionResult> Dashboard()
    {
        var data = await _dashboardService.GetDashboardAsync();
        return View(data);
    }

    public IActionResult Users()
    {
        return View();
    }

    public IActionResult Reports()
    {
        return View();
    }

    public async Task<IActionResult> AuditLogs(int page = 1, int pageSize = 50)
    {
        var repo = _uow.Repository<AuditLog>();
        var logs = await repo.Query()
            .Include(l => l.User)
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return View(logs);
    }

    public async Task<IActionResult> KpiDashboard()
    {
        var data = await _dashboardService.GetDashboardAsync();
        return View(data);
    }

    [HttpGet]
    public async Task<IActionResult> ExportEmergencies()
    {
        var data = await _exportService.ExportEmergenciesToCsvAsync();
        return File(data, "text/csv", $"emergencias_{DateTime.Now:yyyyMMdd}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> ExportPersons()
    {
        var data = await _exportService.ExportPersonsToCsvAsync();
        return File(data, "text/csv", $"personas_{DateTime.Now:yyyyMMdd}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> ExportDonations()
    {
        var data = await _exportService.ExportDonationsToCsvAsync();
        return File(data, "text/csv", $"donaciones_{DateTime.Now:yyyyMMdd}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> ExportDashboard()
    {
        var dashboard = await _dashboardService.GetDashboardAsync();
        var data = await _exportService.ExportDashboardToPdfAsync(dashboard);
        return File(data, "text/html", $"dashboard_{DateTime.Now:yyyyMMdd}.html");
    }

    [HttpGet]
    public IActionResult CreateAlert()
    {
        return View(new CreateAlertDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAlert(CreateAlertDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        await _alertService.CreateAsync(dto, userId);
        TempData["SuccessMessage"] = "Alerta publicada exitosamente.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public IActionResult ManageServices()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateServiceStatus(UpdateServiceStatusDto dto)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(ManageServices));
        var repo = _uow.Repository<ServiceStatus>();
        var existing = await repo.FirstOrDefaultAsync(s => s.ServiceType == dto.ServiceType && s.ZoneCode == dto.ZoneCode);
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (existing is not null)
        {
            existing.StatusType = dto.StatusType;
            existing.Description = dto.Description;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedById = userId;
            await repo.UpdateAsync(existing);
        }
        else
        {
            await repo.AddAsync(new ServiceStatus
            {
                ServiceType = dto.ServiceType,
                StatusType = dto.StatusType,
                ZoneCode = dto.ZoneCode,
                Description = dto.Description,
                UpdatedById = userId
            });
        }
        await _uow.SaveChangesAsync();
        TempData["SuccessMessage"] = "Estado de servicio actualizado.";
        return RedirectToAction(nameof(Dashboard));
    }
}
