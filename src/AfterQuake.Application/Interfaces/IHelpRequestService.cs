using AfterQuake.Application.DTOs;

namespace AfterQuake.Application.Interfaces;

public interface IHelpRequestService
{
    Task<HelpRequestDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<HelpRequestDto>> GetAllAsync();
    Task<IReadOnlyList<HelpRequestDto>> GetPendingAsync();
    Task<IReadOnlyList<HelpRequestDto>> GetUrgentAsync();
    Task<HelpRequestDto> CreateAsync(CreateHelpRequestDto dto, string? userId = null);
    Task AssignAsync(Guid id, Guid volunteerId);
    Task ResolveAsync(Guid id, string? notes);
    Task<int> GetPendingCountAsync();
}
