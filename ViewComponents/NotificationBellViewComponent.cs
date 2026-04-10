using HomeNursingSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HomeNursingSystem.Models;

namespace HomeNursingSystem.ViewComponents;

public class NotificationBellViewComponent : ViewComponent
{
    private readonly INotificationService _notifications;
    private readonly UserManager<ApplicationUser> _users;

    public NotificationBellViewComponent(INotificationService notifications, UserManager<ApplicationUser> users)
    {
        _notifications = notifications;
        _users = users;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = await _users.GetUserAsync(HttpContext.User);
        if (user == null)
            return Content(string.Empty);

        var unread = await _notifications.GetUnreadCountAsync(user.Id);
        var recent = await _notifications.GetRecentAsync(user.Id, 8);
        return View((unread, recent));
    }
}
