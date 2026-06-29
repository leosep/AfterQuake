using Microsoft.EntityFrameworkCore;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Infrastructure.Services;

public class AlertService : IAlertService
{
    private readonly IUnitOfWork _uow;

    public AlertService(IUnitOfWork uow) => _uow = uow;

    public async Task<AlertDto?> GetByIdAsync(Guid id)
    {
        var repo = _uow.Repository<Alert>();
        var entity = await repo.GetByIdAsync(id);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<IReadOnlyList<AlertDto>> GetActiveAsync()
    {
        var repo = _uow.Repository<Alert>();
        var entities = await repo.FindAsync(a => a.IsActive);
        return entities.OrderByDescending(a => a.PublishedAt).Select(MapToDto).ToList();
    }

    public async Task<AlertDto> CreateAsync(CreateAlertDto dto, string? userId = null)
    {
        var entity = new Alert
        {
            AlertType = dto.AlertType,
            Severity = dto.Severity,
            Title = dto.Title,
            Message = dto.Message,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            ZoneCode = dto.ZoneCode,
            ExpiresAt = dto.ExpiresAt,
            RequiresAcknowledgement = dto.RequiresAcknowledgement,
            PublishedById = userId
        };

        var repo = _uow.Repository<Alert>();
        await repo.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task DeactivateAsync(Guid id)
    {
        var repo = _uow.Repository<Alert>();
        var entity = await repo.GetByIdAsync(id);
        if (entity is not null)
        {
            entity.IsActive = false;
            await repo.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<AlertDto?> GetLatestAsync()
    {
        var repo = _uow.Repository<Alert>();
        var entity = await repo.Query()
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.PublishedAt)
            .FirstOrDefaultAsync();
        return entity is null ? null : MapToDto(entity);
    }

    private static AlertDto MapToDto(Alert e) => new()
    {
        Id = e.Id,
        AlertType = e.AlertType,
        AlertTypeName = e.AlertType.ToString(),
        Severity = e.Severity,
        SeverityName = e.Severity.ToString(),
        Title = e.Title,
        Message = e.Message,
        Latitude = e.Latitude,
        Longitude = e.Longitude,
        ZoneCode = e.ZoneCode,
        IsActive = e.IsActive,
        PublishedAt = e.PublishedAt,
        ExpiresAt = e.ExpiresAt
    };
}
