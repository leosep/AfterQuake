using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Enumerations;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Web.Controllers;

public class PersonController : Controller
{
    private readonly IPersonReportService _personReportService;
    private readonly IUnitOfWork _uow;

    public PersonController(IPersonReportService personReportService, IUnitOfWork uow)
    {
        _personReportService = personReportService;
        _uow = uow;
    }

    public async Task<IActionResult> Index(PersonSearchDto search, int page = 1)
    {
        var result = await _personReportService.SearchAsync(search, page);
        return View(result);
    }

    [AllowAnonymous]
    public IActionResult ReportMissing()
    {
        return View(new CreatePersonReportDto());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReportMissing(CreatePersonReportDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var matches = await _personReportService.GetPotentialMatchesAsync(dto.MissingPersonName ?? "");
        if (matches.Any())
        {
            TempData["PotentialMatches"] = System.Text.Json.JsonSerializer.Serialize(matches.Take(5).ToList());
        }

        var userId = User.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;
        await _personReportService.CreateAsync(dto, userId);
        TempData["SuccessMessage"] = "Reporte registrado. Notificaremos si encontramos coincidencias.";
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    public IActionResult ReportFound()
    {
        return View(new ReportFoundDto());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReportFound(ReportFoundDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var userId = User.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;
        await _personReportService.ReportFoundAsync(dto, userId);
        TempData["SuccessMessage"] = "¡Gracias! La persona ha sido marcada como encontrada.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var report = await _personReportService.GetByIdAsync(id);
        if (report is null) return NotFound();
        return View(report);
    }

    [AllowAnonymous]
    public IActionResult ImSafe()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AmSafe()
    {
        TempData["SuccessMessage"] = "¡Qué bueno que estás a salvo! Tu estado ha sido registrado.";
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    public async Task<IActionResult> HospitalPatients()
    {
        var repo = _uow.Repository<UnidentifiedPatient>();
        var patients = await repo.Query().Where(p => !p.IsIdentified).OrderByDescending(p => p.AdmittedAt).ToListAsync();
        return View(patients);
    }

    [Authorize(Roles = "ReliefOrganization,Administrator,SuperAdministrator")]
    public IActionResult RegisterHospitalPatient()
    {
        return View(new UnidentifiedPatient());
    }

    [Authorize(Roles = "ReliefOrganization,Administrator,SuperAdministrator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterHospitalPatient(UnidentifiedPatient patient)
    {
        if (!ModelState.IsValid) return View(patient);
        var repo = _uow.Repository<UnidentifiedPatient>();
        await repo.AddAsync(patient);
        await _uow.SaveChangesAsync();
        TempData["SuccessMessage"] = "Paciente registrado en el hospital. Si alguien lo busca, podrá encontrarlo.";
        return RedirectToAction(nameof(HospitalPatients));
    }
}
