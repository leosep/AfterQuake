using AfterQuake.Application.DTOs;

namespace AfterQuake.Application.Interfaces;

public interface IPersonReportService
{
    Task<PersonReportDto?> GetByIdAsync(Guid id);
    Task<PagedResult<PersonReportDto>> SearchAsync(PersonSearchDto search, int page = 1, int pageSize = 20);
    Task<IReadOnlyList<PersonReportDto>> GetPotentialMatchesAsync(string name);
    Task<PersonReportDto> CreateAsync(CreatePersonReportDto dto, string? userId = null);
    Task ReportFoundAsync(ReportFoundDto dto, string? userId = null);
    Task<int> GetActiveMissingCountAsync();
    Task<int> GetFoundCountAsync();
}
