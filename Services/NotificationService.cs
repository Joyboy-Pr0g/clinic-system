using HomeNursingSystem.Data;
using HomeNursingSystem.Models;
using HomeNursingSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;

    public NotificationService(ApplicationDbContext db) => _db = db;

    public async Task NotifyAsync(string userId, string title, string message, string? link = null, CancellationToken ct = default)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Link = link,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }

    public Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default) =>
        _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, ct);

    public async Task<IReadOnlyList<NotificationItemVM>> GetRecentAsync(string userId, int take, CancellationToken ct = default)
    {
        var list = await _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .Select(n => new NotificationItemVM
            {
                NotificationId = n.NotificationId,
                Title = n.Title,
                Message = n.Message,
                Link = n.Link,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(ct);
        return list;
    }

    public async Task MarkAllReadAsync(string userId, CancellationToken ct = default)
    {
        await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
    }

    public async Task MarkReadAsync(int notificationId, string userId, CancellationToken ct = default)
    {
        await _db.Notifications
            .Where(n => n.NotificationId == notificationId && n.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
    }
}
