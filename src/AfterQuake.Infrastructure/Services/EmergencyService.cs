using Microsoft.EntityFrameworkCore;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Infrastructure.Services;

public class EmergencyService : IEmergencyService
{
    private readonly IUnitOfWork _uow;
    private readonly IEmergencyBroadcastService _broadcastService;

    public EmergencyService(IUnitOfWork uow, IEmergencyBroadcastService broadcastService)
    {
        _uow = uow;
        _broadcastService = broadcastService;
    }

    public async Task<EmergencyReportDto?> GetByIdAsync(Guid id)
    {
        var repo = _uow.Repository<EmergencyReport>();
        var entity = await repo.GetByIdAsync(id);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<IReadOnlyList<EmergencyReportDto>> GetAllAsync()
    {
        var repo = _uow.Repository<EmergencyReport>();
        var entities = await repo.Query().OrderByDescending(e => e.ReportedAt).ToListAsync();
        return entities.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<EmergencyReportDto>> GetActiveAsync()
    {
        var repo = _uow.Repository<EmergencyReport>();
        var entities = await repo.FindAsync(e => e.IsActive);
        return entities.OrderByDescending(e => e.ReportedAt).Select(MapToDto).ToList();
    }

    public async Task<EmergencyReportDto> CreateAsync(CreateEmergencyReportDto dto, string? userId = null)
    {
        var entity = new EmergencyReport
        {
            Description = dto.Description,
            EmergencyType = dto.EmergencyType,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Address = dto.Address,
            ZoneCode = dto.ZoneCode,
            AffectedPeople = dto.AffectedPeople,
            RequiresImmediateRescue = dto.RequiresImmediateRescue,
            ReporterPhone = dto.ReporterPhone,
            UserId = userId
        };

        var repo = _uow.Repository<EmergencyReport>();
        await repo.AddAsync(entity);
        await _uow.SaveChangesAsync();

        var resultDto = MapToDto(entity);
        await _broadcastService.BroadcastEmergencyAsync(resultDto);

        return resultDto;
    }

    public async Task ResolveAsync(Guid id, string? resolutionNotes)
    {
        var repo = _uow.Repository<EmergencyReport>();
        var entity = await repo.GetByIdAsync(id);
        if (entity is not null)
        {
            entity.IsActive = false;
            entity.ResolvedAt = DateTime.UtcNow;
            entity.ResolutionNotes = resolutionNotes;
            await repo.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<int> GetActiveCountAsync()
    {
        var repo = _uow.Repository<EmergencyReport>();
        return await repo.CountAsync(e => e.IsActive);
    }

    private static EmergencyReportDto MapToDto(EmergencyReport e) => new()
    {
        Id = e.Id,
        Description = e.Description,
        EmergencyType = e.EmergencyType,
        EmergencyTypeName = e.EmergencyType.ToString(),
        Latitude = e.Latitude,
        Longitude = e.Longitude,
        Address = e.Address,
        ZoneCode = e.ZoneCode,
        AffectedPeople = e.AffectedPeople,
        RequiresImmediateRescue = e.RequiresImmediateRescue,
        ReporterPhone = e.ReporterPhone,
        ReportedAt = e.ReportedAt,
        IsActive = e.IsActive,
        ResolvedAt = e.ResolvedAt
    };
}
