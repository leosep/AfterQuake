using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;

namespace AfterQuake.Web.Controllers.Api;

[ApiController]
[Authorize]
[EnableRateLimiting("ApiRateLimit")]
[Route("api/emergency")]
public class EmergencyApiController : ControllerBase
{
    private readonly IEmergencyService _emergencyService;

    public EmergencyApiController(IEmergencyService emergencyService) => _emergencyService = emergencyService;

    /// <summary>
    /// Lista todas las emergencias activas.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<EmergencyReportDto>>> GetActive()
    {
        var emergencies = await _emergencyService.GetActiveAsync();
        return Ok(emergencies);
    }

    /// <summary>
    /// Obtiene una emergencia por su ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<EmergencyReportDto>> GetById(Guid id)
    {
        var emergency = await _emergencyService.GetByIdAsync(id);
        if (emergency is null)
            return NotFound(new { Message = "Emergencia no encontrada." });
        return Ok(emergency);
    }

    /// <summary>
    /// Reporta una nueva emergencia (sin autenticación requerida, con rate limit).
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("EmergencyPost")]
    public async Task<ActionResult<EmergencyReportDto>> Create([FromBody] CreateEmergencyReportDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _emergencyService.CreateAsync(dto, userId: null);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
