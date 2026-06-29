using AfterQuake.Domain.Entities;

namespace AfterQuake.Application.Interfaces;

public interface INotificationService
{
    Task SendToUserAsync(string userId, string title, string message, string? link = null);
    Task SendToRoleAsync(string role, string title, string message, string? link = null);
    Task SendToZoneAsync(string zoneCode, string title, string message, string? link = null);
    Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(string userId);
    Task MarkAsReadAsync(Guid notificationId);
    Task<int> GetUnreadCountAsync(string userId);
}
