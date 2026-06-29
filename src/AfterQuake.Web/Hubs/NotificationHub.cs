using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AfterQuake.Web.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public override async Task OnConnectedAsync()
    {
        if (Context.User?.Identity?.IsAuthenticated == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{Context.UserIdentifier}");
        }
        await base.OnConnectedAsync();
    }
}
