using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Web.Controllers.Api;

public class NearbyPlaceResult
{
    public string Type { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public string? ZoneCode { get; set; }
    public double DistanceKm { get; set; }
    public int? AvailableCapacity { get; set; }
    public bool? HasMedicalPost { get; set; }
    public bool? HasFoodSupply { get; set; }
    public bool? HasWater { get; set; }
    public bool? HasElectricity { get; set; }
    public int? AvailableBeds { get; set; }
    public bool? HasEmergencyRoom { get; set; }
    public string? ContactPhone { get; set; }
    public string? OperatingHours { get; set; }
    public string? NeededItems { get; set; }
}

[ApiController]
[Authorize]
[EnableRateLimiting("ApiRateLimit")]
[Route("api/map")]
public class MapApiController : ControllerBase
{
    private readonly IShelterService _shelterService;
    private readonly IUnitOfWork _uow;
    private readonly IHaversineService _haversine;

    public MapApiController(IShelterService shelterService, IUnitOfWork uow, IHaversineService haversine)
    {
        _shelterService = shelterService;
        _uow = uow;
        _haversine = haversine;
    }

    [HttpGet("shelters")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ShelterDto>>> GetShelters()
    {
        var shelters = await _shelterService.GetActiveAsync();
        return Ok(shelters);
    }

    [HttpGet("health-centers")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<object>>> GetHealthCenters()
    {
        var repo = _uow.Repository<HealthCenter>();
        var centers = await repo.FindAsync(c => c.IsOperational);

        var result = centers.Select(c => new
        {
            c.Id,
            c.Name,
            c.Description,
            c.Latitude,
            c.Longitude,
            c.Address,
            c.ZoneCode,
            c.ContactPhone,
            c.ContactEmail,
            c.HasEmergencyRoom,
            c.HasSurgeryCapacity,
            AvailableBeds = c.AvailableBeds,
            TotalBeds = c.TotalBeds,
            c.Services,
            c.Specializations
        }).ToList();

        return Ok(result);
    }

    [HttpGet("donation-points")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<object>>> GetDonationPoints()
    {
        var repo = _uow.Repository<DonationPoint>();
        var points = await repo.FindAsync(d => d.IsActive);

        var result = points.Select(d => new
        {
            d.Id,
            d.Name,
            d.Description,
            d.Latitude,
            d.Longitude,
            d.Address,
            d.ZoneCode,
            d.ContactPhone,
            d.OperatingHours,
            d.NeededItems,
            d.UrgentlyNeededItems
        }).ToList();

        return Ok(result);
    }

    [HttpGet("nearby")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetNearby(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radius = 10)
    {
        var shelterRepo = _uow.Repository<Shelter>();
        var healthRepo = _uow.Repository<HealthCenter>();
        var donationRepo = _uow.Repository<DonationPoint>();

        var shelters = await shelterRepo.FindAsync(s => s.Status == Domain.Enumerations.ShelterStatus.Active || s.Status == Domain.Enumerations.ShelterStatus.Full);
        var healthCenters = await healthRepo.FindAsync(c => c.IsOperational);
        var donationPoints = await donationRepo.FindAsync(d => d.IsActive);

        var nearbyShelters = shelters
            .Where(s => _haversine.IsWithinRadius(lat, lng, s.Latitude, s.Longitude, radius))
            .Select(s => new NearbyPlaceResult
            {
                Type = "shelter",
                Id = s.Id,
                Name = s.Name,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                Address = s.Address,
                ZoneCode = s.ZoneCode,
                AvailableCapacity = s.AvailableCapacity,
                HasMedicalPost = s.HasMedicalPost,
                HasFoodSupply = s.HasFoodSupply,
                HasWater = s.HasWater,
                HasElectricity = s.HasElectricity,
                DistanceKm = Math.Round(_haversine.CalculateDistance(lat, lng, s.Latitude, s.Longitude), 2)
            });

        var nearbyHealthCenters = healthCenters
            .Where(c => _haversine.IsWithinRadius(lat, lng, c.Latitude, c.Longitude, radius))
            .Select(c => new NearbyPlaceResult
            {
                Type = "health_center",
                Id = c.Id,
                Name = c.Name,
                Latitude = c.Latitude,
                Longitude = c.Longitude,
                Address = c.Address,
                ZoneCode = c.ZoneCode,
                AvailableBeds = c.AvailableBeds,
                HasEmergencyRoom = c.HasEmergencyRoom,
                ContactPhone = c.ContactPhone,
                DistanceKm = Math.Round(_haversine.CalculateDistance(lat, lng, c.Latitude, c.Longitude), 2)
            });

        var nearbyDonationPoints = donationPoints
            .Where(d => _haversine.IsWithinRadius(lat, lng, d.Latitude, d.Longitude, radius))
            .Select(d => new NearbyPlaceResult
            {
                Type = "donation_point",
                Id = d.Id,
                Name = d.Name,
                Latitude = d.Latitude,
                Longitude = d.Longitude,
                Address = d.Address,
                ZoneCode = d.ZoneCode,
                OperatingHours = d.OperatingHours,
                NeededItems = d.NeededItems,
                DistanceKm = Math.Round(_haversine.CalculateDistance(lat, lng, d.Latitude, d.Longitude), 2)
            });

        var all = nearbyShelters
            .Concat(nearbyHealthCenters)
            .Concat(nearbyDonationPoints)
            .OrderBy(r => r.DistanceKm)
            .ToList();

        return Ok(new
        {
            Latitude = lat,
            Longitude = lng,
            RadiusKm = radius,
            Results = all,
            TotalCount = all.Count
        });
    }
}
