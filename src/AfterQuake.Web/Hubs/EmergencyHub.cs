using Microsoft.AspNetCore.SignalR;

namespace AfterQuake.Web.Hubs;

public class EmergencyHub : Hub
{
    public async Task SubscribeToZone(string zoneCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"zone_{zoneCode}");
    }

    public async Task UnsubscribeFromZone(string zoneCode)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"zone_{zoneCode}");
    }

    public async Task SendEmergencyAlert(string zoneCode, string message)
    {
        await Clients.Group($"zone_{zoneCode}").SendAsync("ReceiveEmergencyAlert", message);
    }
}
