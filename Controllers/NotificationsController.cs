using HomeNursingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HomeNursingSystem.Models;

namespace HomeNursingSystem.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notifications;
    private readonly UserManager<ApplicationUser> _users;

    public NotificationsController(INotificationService notifications, UserManager<ApplicationUser> users)
    {
        _notifications = notifications;
        _users = users;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        if (user != null)
            await _notifications.MarkAllReadAsync(user.Id, ct);
        return RedirectToAction("Index", "Home");
    }
}
