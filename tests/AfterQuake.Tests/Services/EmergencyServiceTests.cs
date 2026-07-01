using Microsoft.EntityFrameworkCore;
using AfterQuake.Application.DTOs;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Enumerations;
using AfterQuake.Domain.Interfaces;
using AfterQuake.Infrastructure.Data;
using AfterQuake.Infrastructure.Services;

namespace AfterQuake.Tests.Services;

public class EmergencyServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _uow;
    private readonly EmergencyService _service;

    public EmergencyServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _uow = new UnitOfWork(_context);
        _service = new EmergencyService(_uow);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateAsync_ShouldReturnDtoWithCorrectData()
    {
        var dto = new CreateEmergencyReportDto
        {
            Description = "Incendio en edificio residencial",
            EmergencyType = EmergencyType.Fire,
            Latitude = 18.4861,
            Longitude = -69.9312,
            Address = "Av. Independencia 123, Santo Domingo",
            ZoneCode = "ZONA-DN",
            AffectedPeople = 10,
            RequiresImmediateRescue = true,
            ReporterPhone = "+18095554321"
        };

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(EmergencyType.Fire, result.EmergencyType);
        Assert.Equal(dto.Latitude, result.Latitude);
        Assert.Equal(dto.Longitude, result.Longitude);
        Assert.Equal(dto.AffectedPeople, result.AffectedPeople);
        Assert.True(result.IsActive);
        Assert.True(result.RequiresImmediateRescue);
    }

    [Fact]
    public async Task GetActiveAsync_ShouldReturnOnlyActiveEmergencies()
    {
        var repo = _uow.Repository<EmergencyReport>();
        var active1 = new EmergencyReport { Description = "Active 1", EmergencyType = EmergencyType.Other, IsActive = true, ReportedAt = DateTime.UtcNow };
        var active2 = new EmergencyReport { Description = "Active 2", EmergencyType = EmergencyType.Other, IsActive = true, ReportedAt = DateTime.UtcNow };
        var resolved = new EmergencyReport { Description = "Resolved", EmergencyType = EmergencyType.Other, IsActive = false, ResolvedAt = DateTime.UtcNow };
        await repo.AddAsync(active1);
        await repo.AddAsync(active2);
        await repo.AddAsync(resolved);
        await _uow.SaveChangesAsync();

        var result = await _service.GetActiveAsync();

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.True(r.IsActive));
    }

    [Fact]
    public async Task ResolveAsync_ShouldSetIsActiveToFalse()
    {
        var repo = _uow.Repository<EmergencyReport>();
        var emergency = new EmergencyReport { Description = "To resolve", EmergencyType = EmergencyType.Flood, IsActive = true, ReportedAt = DateTime.UtcNow };
        await repo.AddAsync(emergency);
        await _uow.SaveChangesAsync();

        await _service.ResolveAsync(emergency.Id, "Resuelto - Brigada en terreno");

        var updated = await repo.GetByIdAsync(emergency.Id);
        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
        Assert.NotNull(updated.ResolvedAt);
        Assert.Equal("Resuelto - Brigada en terreno", updated.ResolutionNotes);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullWhenNotFound()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveCountAsync_ShouldReturnCorrectCount()
    {
        var repo = _uow.Repository<EmergencyReport>();
        await repo.AddAsync(new EmergencyReport { Description = "E1", EmergencyType = EmergencyType.Other, IsActive = true, ReportedAt = DateTime.UtcNow });
        await repo.AddAsync(new EmergencyReport { Description = "E2", EmergencyType = EmergencyType.Other, IsActive = true, ReportedAt = DateTime.UtcNow });
        await repo.AddAsync(new EmergencyReport { Description = "E3", EmergencyType = EmergencyType.Other, IsActive = false, ReportedAt = DateTime.UtcNow });
        await _uow.SaveChangesAsync();

        var count = await _service.GetActiveCountAsync();

        Assert.Equal(2, count);
    }
}
