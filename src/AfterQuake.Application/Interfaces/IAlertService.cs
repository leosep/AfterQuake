using AfterQuake.Application.DTOs;

namespace AfterQuake.Application.Interfaces;

public interface IAlertService
{
    Task<AlertDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<AlertDto>> GetActiveAsync();
    Task<AlertDto> CreateAsync(CreateAlertDto dto, string? userId = null);
    Task DeactivateAsync(Guid id);
    Task<AlertDto?> GetLatestAsync();
}
