using AfterQuake.Application.DTOs;

namespace AfterQuake.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync();
}
