using Microsoft.EntityFrameworkCore;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Enumerations;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Infrastructure.Services;

public class HelpRequestService : IHelpRequestService
{
    private readonly IUnitOfWork _uow;

    public HelpRequestService(IUnitOfWork uow) => _uow = uow;

    public async Task<HelpRequestDto?> GetByIdAsync(Guid id)
    {
        var repo = _uow.Repository<HelpRequest>();
        var entity = await repo.GetByIdAsync(id);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<IReadOnlyList<HelpRequestDto>> GetAllAsync()
    {
        var repo = _uow.Repository<HelpRequest>();
        var entities = await repo.Query().OrderByDescending(e => e.RequestedAt).ToListAsync();
        return entities.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<HelpRequestDto>> GetPendingAsync()
    {
        var repo = _uow.Repository<HelpRequest>();
        var entities = await repo.FindAsync(e => e.Status == HelpRequestStatus.Pending);
        return entities.OrderByDescending(e => e.Priority).ThenBy(e => e.RequestedAt).Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<HelpRequestDto>> GetUrgentAsync()
    {
        var repo = _uow.Repository<HelpRequest>();
        var entities = await repo.FindAsync(e => e.IsUrgent && e.Status != HelpRequestStatus.Resolved);
        return entities.OrderByDescending(e => e.Priority).Select(MapToDto).ToList();
    }

    public async Task<HelpRequestDto> CreateAsync(CreateHelpRequestDto dto, string? userId = null)
    {
        var entity = new HelpRequest
        {
            RequestType = dto.RequestType,
            Description = dto.Description,
            PeopleCount = dto.PeopleCount,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Address = dto.Address,
            ZoneCode = dto.ZoneCode,
            RequesterName = dto.RequesterName,
            RequesterPhone = dto.RequesterPhone,
            RequesterEmail = dto.RequesterEmail,
            IsUrgent = dto.IsUrgent,
            Priority = dto.IsUrgent ? HelpRequestPriority.High : HelpRequestPriority.Medium,
            UserId = userId
        };

        var repo = _uow.Repository<HelpRequest>();
        await repo.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task AssignAsync(Guid id, Guid volunteerId)
    {
        var repo = _uow.Repository<HelpRequest>();
        var entity = await repo.GetByIdAsync(id);
        if (entity is not null)
        {
            entity.AssignedToVolunteerId = volunteerId;
            entity.Status = HelpRequestStatus.Assigned;
            await repo.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task ResolveAsync(Guid id, string? notes)
    {
        var repo = _uow.Repository<HelpRequest>();
        var entity = await repo.GetByIdAsync(id);
        if (entity is not null)
        {
            entity.Status = HelpRequestStatus.Resolved;
            entity.ResolvedAt = DateTime.UtcNow;
            entity.ResolutionNotes = notes;
            await repo.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<int> GetPendingCountAsync()
    {
        var repo = _uow.Repository<HelpRequest>();
        return await repo.CountAsync(e => e.Status == HelpRequestStatus.Pending);
    }

    private static HelpRequestDto MapToDto(HelpRequest e) => new()
    {
        Id = e.Id,
        RequestType = e.RequestType,
        RequestTypeName = e.RequestType.ToString(),
        Priority = e.Priority,
        PriorityName = e.Priority.ToString(),
        Status = e.Status,
        StatusName = e.Status.ToString(),
        Description = e.Description,
        PeopleCount = e.PeopleCount,
        Latitude = e.Latitude,
        Longitude = e.Longitude,
        Address = e.Address,
        ZoneCode = e.ZoneCode,
        RequesterName = e.RequesterName,
        RequesterPhone = e.RequesterPhone,
        RequesterEmail = e.RequesterEmail,
        RequestedAt = e.RequestedAt,
        IsUrgent = e.IsUrgent,
        ResolvedAt = e.ResolvedAt
    };
}
