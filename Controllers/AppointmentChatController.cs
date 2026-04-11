using HomeNursingSystem.Models;
using HomeNursingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HomeNursingSystem.Controllers;

[Authorize]
[Route("appointment-chat")]
public class AppointmentChatController : Controller
{
    private readonly IAppointmentChatService _chat;
    private readonly IFileUploadService _files;
    private readonly UserManager<ApplicationUser> _users;

    public AppointmentChatController(
        IAppointmentChatService chat,
        IFileUploadService files,
        UserManager<ApplicationUser> users)
    {
        _chat = chat;
        _files = files;
        _users = users;
    }

    [HttpGet("{appointmentId:int}/messages")]
    public async Task<IActionResult> Messages(int appointmentId, int? after, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var list = await _chat.GetMessagesAsync(appointmentId, user!.Id, after, ct);
        return Json(new { messages = list });
    }

    [HttpPost("{appointmentId:int}/messages")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<IActionResult> Post(int appointmentId, [FromForm] string? body, [FromForm] IFormFile? file, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        string? attachmentUrl = null;
        if (file != null && file.Length > 0)
        {
            attachmentUrl = await _files.SaveChatAttachmentAsync(file, ct);
            if (attachmentUrl == null)
                return BadRequest(new { error = "الملف غير مدعوم أو أكبر من 15 ميجابايت." });
        }

        var (ok, err) = await _chat.SendAsync(appointmentId, user!.Id, body, attachmentUrl, ct);
        if (!ok)
            return BadRequest(new { error = err });
        return Json(new { ok = true });
    }
}
