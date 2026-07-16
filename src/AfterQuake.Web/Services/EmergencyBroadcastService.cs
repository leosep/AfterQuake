using Microsoft.AspNetCore.SignalR;
using AfterQuake.Application.DTOs;
using AfterQuake.Application.Interfaces;
using AfterQuake.Web.Hubs;

namespace AfterQuake.Web.Services;

public class EmergencyBroadcastService : IEmergencyBroadcastService
{
    private readonly IHubContext<EmergencyHub> _hubContext;

    public EmergencyBroadcastService(IHubContext<EmergencyHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastEmergencyAsync(EmergencyReportDto dto)
    {
        var zoneGroup = string.IsNullOrEmpty(dto.ZoneCode) ? "zone_general" : $"zone_{dto.ZoneCode}";
        await _hubContext.Clients.Group(zoneGroup).SendAsync("ReceiveEmergencyAlert", dto);
    }

    public async Task BroadcastAlertAsync(AlertDto dto)
    {
        var zoneGroup = string.IsNullOrEmpty(dto.ZoneCode) ? "zone_general" : $"zone_{dto.ZoneCode}";
        await _hubContext.Clients.Group(zoneGroup).SendAsync("ReceiveEmergencyAlert", dto);
    }
}
