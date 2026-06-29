using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AfterQuake.Application.Interfaces;
using AfterQuake.Domain.Entities;
using AfterQuake.Domain.Interfaces;

namespace AfterQuake.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationService(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public async Task SendToUserAsync(string userId, string title, string message, string? link = null)
    {
        var repo = _uow.Repository<Notification>();
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Link = link,
            Type = "User"
        };
        await repo.AddAsync(notification);
        await _uow.SaveChangesAsync();
    }

    public async Task SendToRoleAsync(string role, string title, string message, string? link = null)
    {
        var repo = _uow.Repository<Notification>();
        var users = await _userManager.GetUsersInRoleAsync(role);
        foreach (var user in users.Where(u => u.IsActive))
        {
            var notification = new Notification
            {
                UserId = user.Id,
                Title = title,
                Message = message,
                Link = link,
                Type = "Role"
            };
            await repo.AddAsync(notification);
        }
        await _uow.SaveChangesAsync();
    }

    public async Task SendToZoneAsync(string zoneCode, string title, string message, string? link = null)
    {
        var repo = _uow.Repository<Notification>();
        var emergencyRepo = _uow.Repository<EmergencyReport>();
        var personRepo = _uow.Repository<PersonReport>();
        var helpRepo = _uow.Repository<HelpRequest>();

        var zoneUserIds = await emergencyRepo.Query()
            .Where(e => e.ZoneCode == zoneCode && e.UserId != null)
            .Select(e => e.UserId!)
            .Distinct()
            .Union(personRepo.Query()
                .Where(p => p.ZoneCode == zoneCode && p.UserId != null)
                .Select(p => p.UserId!))
            .Union(helpRepo.Query()
                .Where(h => h.ZoneCode == zoneCode && h.UserId != null)
                .Select(h => h.UserId!))
            .Distinct()
            .ToListAsync();

        foreach (var userId in zoneUserIds)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Link = link,
                Type = "Zone"
            };
            await repo.AddAsync(notification);
        }
        await _uow.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(string userId)
    {
        var repo = _uow.Repository<Notification>();
        return await repo.FindAsync(n => n.UserId == userId);
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var repo = _uow.Repository<Notification>();
        var notification = await repo.GetByIdAsync(notificationId);
        if (notification is not null)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await repo.UpdateAsync(notification);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        var repo = _uow.Repository<Notification>();
        return await repo.CountAsync(n => n.UserId == userId && !n.IsRead);
    }
}
