using AfterQuake.Application.DTOs;

namespace AfterQuake.Application.Interfaces;

public interface IShelterService
{
    Task<ShelterDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<ShelterDto>> GetAllAsync();
    Task<IReadOnlyList<ShelterDto>> GetActiveAsync();
    Task<ShelterDto> CreateAsync(CreateShelterDto dto, string? userId = null);
    Task UpdateCapacityAsync(Guid id, UpdateShelterCapacityDto dto);
    Task UpdateStatusAsync(Guid id, Domain.Enumerations.ShelterStatus status);
    Task<int> GetAvailableCapacityTotalAsync();
}
