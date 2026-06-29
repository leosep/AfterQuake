using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Enumerations;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Web.Controllers.Api;

[ApiController]
[Authorize]
[EnableRateLimiting("ApiRateLimit")]
[Route("api/help")]
public class HelpApiController : ControllerBase
{
    private readonly IHelpRequestService _helpRequestService;
    private readonly IUnitOfWork _uow;

    public HelpApiController(IHelpRequestService helpRequestService, IUnitOfWork uow)
    {
        _helpRequestService = helpRequestService;
        _uow = uow;
    }

    [HttpGet("requests")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<HelpRequestDto>>> GetRequests()
    {
        var requests = await _helpRequestService.GetAllAsync();
        return Ok(requests);
    }

    [HttpPost("requests")]
    [AllowAnonymous]
    public async Task<ActionResult<HelpRequestDto>> CreateRequest([FromBody] CreateHelpRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _helpRequestService.CreateAsync(dto, userId: null);
        return CreatedAtAction(nameof(GetRequests), null, result);
    }

    [HttpGet("offers")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<object>>> GetOffers()
    {
        var repo = _uow.Repository<HelpOffer>();
        var offers = await repo.FindAsync(o => o.IsAvailable);

        var result = offers.Select(o => new
        {
            o.Id,
            OfferType = o.OfferType.ToString(),
            o.Description,
            o.Quantity,
            o.Latitude,
            o.Longitude,
            o.Address,
            o.ZoneCode,
            o.ContactName,
            o.ContactPhone,
            o.ContactEmail,
            o.OfferedAt
        }).ToList();

        return Ok(result);
    }

    [HttpPost("offers")]
    [AllowAnonymous]
    public async Task<ActionResult> CreateOffer([FromBody] CreateHelpOfferDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = new HelpOffer
        {
            OfferType = dto.OfferType,
            Description = dto.Description,
            Quantity = dto.Quantity,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Address = dto.Address,
            ZoneCode = dto.ZoneCode,
            ContactName = dto.ContactName,
            ContactPhone = dto.ContactPhone,
            ContactEmail = dto.ContactEmail
        };

        var repo = _uow.Repository<HelpOffer>();
        await repo.AddAsync(entity);
        await _uow.SaveChangesAsync();

        return Created($"/api/help/offers/{entity.Id}", new
        {
            entity.Id,
            OfferType = entity.OfferType.ToString(),
            entity.Description,
            entity.Quantity,
            entity.Latitude,
            entity.Longitude,
            entity.Address,
            entity.ZoneCode,
            entity.ContactName,
            entity.ContactPhone,
            entity.ContactEmail,
            entity.OfferedAt
        });
    }
}

public class CreateHelpOfferDto
{
    public HelpOfferType OfferType { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; } = 1;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public string? ZoneCode { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
}
