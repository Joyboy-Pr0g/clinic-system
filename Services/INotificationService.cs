using HomeNursingSystem.ViewModels;

namespace HomeNursingSystem.Services;

public interface INotificationService
{
    Task NotifyAsync(string userId, string title, string message, string? link = null, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<NotificationItemVM>> GetRecentAsync(string userId, int take, CancellationToken ct = default);
    Task MarkAllReadAsync(string userId, CancellationToken ct = default);
    Task MarkReadAsync(int notificationId, string userId, CancellationToken ct = default);
}
