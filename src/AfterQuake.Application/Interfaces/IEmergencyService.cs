using AfterQuake.Application.DTOs;

namespace AfterQuake.Application.Interfaces;

public interface IEmergencyService
{
    Task<EmergencyReportDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<EmergencyReportDto>> GetAllAsync();
    Task<IReadOnlyList<EmergencyReportDto>> GetActiveAsync();
    Task<EmergencyReportDto> CreateAsync(CreateEmergencyReportDto dto, string? userId = null);
    Task ResolveAsync(Guid id, string? resolutionNotes);
    Task<int> GetActiveCountAsync();
}
