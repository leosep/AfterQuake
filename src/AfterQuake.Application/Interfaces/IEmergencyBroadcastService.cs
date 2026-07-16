using AfterQuake.Application.DTOs;

namespace AfterQuake.Application.Interfaces;

public interface IEmergencyBroadcastService
{
    Task BroadcastEmergencyAsync(EmergencyReportDto dto);
    Task BroadcastAlertAsync(AlertDto dto);
}
