using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Web.Controllers.Api;

[ApiController]
[Authorize]
[EnableRateLimiting("ApiRateLimit")]
[Route("api/persons")]
public class PersonApiController : ControllerBase
{
    private readonly IPersonReportService _personReportService;
    private readonly IUnitOfWork _uow;

    public PersonApiController(IPersonReportService personReportService, IUnitOfWork uow)
    {
        _personReportService = personReportService;
        _uow = uow;
    }

    /// <summary>
    /// Busca reportes de personas con filtros opcionales (nombre, zona, tipo, estado).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<PersonReportDto>>> Search(
        [FromQuery] string? name,
        [FromQuery] string? zone,
        [FromQuery] string? type,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var search = new PersonSearchDto
        {
            Query = name,
            ZoneCode = zone,
            ReportType = !string.IsNullOrWhiteSpace(type) && Enum.TryParse<Domain.Enumerations.PersonReportType>(type, true, out var rt) ? rt : null,
            Status = !string.IsNullOrWhiteSpace(status) && Enum.TryParse<Domain.Enumerations.PersonReportStatus>(status, true, out var s) ? s : null
        };

        var result = await _personReportService.SearchAsync(search, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un reporte de persona por su ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<PersonReportDto>> GetById(Guid id)
    {
        var report = await _personReportService.GetByIdAsync(id);
        if (report is null)
            return NotFound(new { Message = "Reporte no encontrado." });
        return Ok(report);
    }

    /// <summary>
    /// Reporta una persona como desaparecida o encontrada.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<PersonReportDto>> Create([FromBody] CreatePersonReportDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _personReportService.CreateAsync(dto, userId: null);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Lista pacientes hospitalarios no identificados.
    /// </summary>
    [HttpGet("hospital-patients")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<object>>> GetHospitalPatients()
    {
        var repo = _uow.Repository<UnidentifiedPatient>();
        var patients = await repo.FindAsync(p => !p.IsIdentified);

        var result = patients.Select(p => new
        {
            p.Id,
            p.HospitalName,
            p.HospitalContact,
            p.PhotoUrl,
            p.EstimatedAge,
            p.Gender,
            p.PhysicalDescription,
            p.Clothing,
            p.DistinctiveMarks,
            p.MedicalCondition,
            p.ZoneCode,
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            p.AdmittedAt,
            p.Notes
        }).ToList();

        return Ok(result);
    }
}
