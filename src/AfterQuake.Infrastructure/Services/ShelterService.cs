using Microsoft.EntityFrameworkCore;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Enumerations;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Infrastructure.Services;

public class ShelterService : IShelterService
{
    private readonly IUnitOfWork _uow;

    public ShelterService(IUnitOfWork uow) => _uow = uow;

    public async Task<ShelterDto?> GetByIdAsync(Guid id)
    {
        var repo = _uow.Repository<Shelter>();
        var entity = await repo.GetByIdAsync(id);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<IReadOnlyList<ShelterDto>> GetAllAsync()
    {
        var repo = _uow.Repository<Shelter>();
        var entities = await repo.Query().OrderBy(e => e.Name).ToListAsync();
        return entities.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<ShelterDto>> GetActiveAsync()
    {
        var repo = _uow.Repository<Shelter>();
        var entities = await repo.FindAsync(e => e.Status == ShelterStatus.Active);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<ShelterDto> CreateAsync(CreateShelterDto dto, string? userId = null)
    {
        var entity = new Shelter
        {
            Name = dto.Name,
            Description = dto.Description,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Address = dto.Address,
            ZoneCode = dto.ZoneCode,
            TotalCapacity = dto.TotalCapacity,
            HasElectricity = dto.HasElectricity,
            HasWater = dto.HasWater,
            HasMedicalPost = dto.HasMedicalPost,
            ContactPhone = dto.ContactPhone,
            ManagedById = userId
        };

        var repo = _uow.Repository<Shelter>();
        await repo.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task UpdateCapacityAsync(Guid id, UpdateShelterCapacityDto dto)
    {
        var repo = _uow.Repository<Shelter>();
        var entity = await repo.GetByIdAsync(id);
        if (entity is not null)
        {
            entity.CurrentOccupancy = dto.CurrentOccupancy;
            entity.UpdatedAt = DateTime.UtcNow;
            if (entity.CurrentOccupancy >= entity.TotalCapacity)
                entity.Status = ShelterStatus.Full;
            await repo.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task UpdateStatusAsync(Guid id, ShelterStatus status)
    {
        var repo = _uow.Repository<Shelter>();
        var entity = await repo.GetByIdAsync(id);
        if (entity is not null)
        {
            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;
            await repo.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<int> GetAvailableCapacityTotalAsync()
    {
        var repo = _uow.Repository<Shelter>();
        var shelters = await repo.FindAsync(e => e.Status == ShelterStatus.Active || e.Status == ShelterStatus.Full);
        return shelters.Sum(s => s.AvailableCapacity);
    }

    private static ShelterDto MapToDto(Shelter e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        Status = e.Status,
        StatusName = e.Status.ToString(),
        Latitude = e.Latitude,
        Longitude = e.Longitude,
        Address = e.Address,
        ZoneCode = e.ZoneCode,
        TotalCapacity = e.TotalCapacity,
        CurrentOccupancy = e.CurrentOccupancy,
        AvailableCapacity = e.AvailableCapacity,
        HasElectricity = e.HasElectricity,
        HasWater = e.HasWater,
        HasMedicalPost = e.HasMedicalPost,
        HasFoodSupply = e.HasFoodSupply,
        ContactPhone = e.ContactPhone,
        UpdatedAt = e.UpdatedAt ?? e.CreatedAt
    };
}
