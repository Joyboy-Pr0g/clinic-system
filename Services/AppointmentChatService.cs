using HomeNursingSystem.Data;
using HomeNursingSystem.Models;
using HomeNursingSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Services;

public class AppointmentChatService : IAppointmentChatService
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notifications;
    private readonly IAppointmentRealtimeDispatcher _realtime;

    public AppointmentChatService(
        ApplicationDbContext db,
        INotificationService notifications,
        IAppointmentRealtimeDispatcher realtime)
    {
        _db = db;
        _notifications = notifications;
        _realtime = realtime;
    }

    public static bool IsChatOpen(string status) =>
        AppointmentStatuses.ChatAndTrackingUnlocked(status);

    public async Task<IReadOnlyList<AppointmentChatMessageDto>> GetMessagesAsync(int appointmentId, string userId, int? afterMessageId, CancellationToken ct = default)
    {
        var appt = await LoadAppointmentForParticipantAsync(appointmentId, userId, ct);
        if (appt == null || !IsChatOpen(appt.Status))
            return Array.Empty<AppointmentChatMessageDto>();

        var q = _db.AppointmentMessages.AsNoTracking()
            .Where(m => m.AppointmentId == appointmentId);
        if (afterMessageId.HasValue)
            q = q.Where(m => m.AppointmentMessageId > afterMessageId.Value);

        var list = await q
            .Include(m => m.Sender)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new AppointmentChatMessageDto
            {
                AppointmentMessageId = m.AppointmentMessageId,
                SenderUserId = m.SenderUserId,
                SenderName = m.Sender.FullName,
                Body = m.Body,
                MessageType = m.MessageType,
                AttachmentUrl = m.AttachmentUrl,
                CreatedAt = m.CreatedAt,
                IsMine = m.SenderUserId == userId
            })
            .ToListAsync(ct);

        return list;
    }

    public async Task<(bool ok, string? error)> SendAsync(int appointmentId, string userId, string? body, string? attachmentUrl, CancellationToken ct = default)
    {
        body = body?.Trim() ?? "";
        if (body.Length > 4000)
            return (false, "النص طويل جدًا.");

        var hasFile = !string.IsNullOrEmpty(attachmentUrl);
        if (!hasFile && string.IsNullOrEmpty(body))
            return (false, "اكتب نصاً أو أرسل صورة/تسجيل صوتي.");

        var appt = await LoadAppointmentForParticipantAsync(appointmentId, userId, ct);
        if (appt == null)
            return (false, "غير مصرح أو الموعد غير موجود.");
        if (!IsChatOpen(appt.Status))
            return (false, "الدردشة متاحة بعد قبول الموعد.");

        var msgType = "text";
        if (hasFile)
            msgType = ClassifyAttachment(attachmentUrl!);

        var msg = new AppointmentMessage
        {
            AppointmentId = appointmentId,
            SenderUserId = userId,
            Body = body,
            MessageType = msgType,
            AttachmentUrl = attachmentUrl,
            CreatedAt = DateTime.UtcNow
        };
        _db.AppointmentMessages.Add(msg);
        await _db.SaveChangesAsync(ct);

        var senderName = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.FullName)
            .FirstAsync(ct);
        var dto = new AppointmentChatMessageDto
        {
            AppointmentMessageId = msg.AppointmentMessageId,
            SenderUserId = userId,
            SenderName = senderName,
            Body = msg.Body,
            MessageType = msg.MessageType,
            AttachmentUrl = msg.AttachmentUrl,
            CreatedAt = msg.CreatedAt,
            IsMine = false
        };
        await _realtime.BroadcastChatMessageAsync(appointmentId, dto, ct);

        var preview = msgType switch
        {
            "image" => string.IsNullOrEmpty(body) ? "📷 صورة" : $"📷 {body[..Math.Min(60, body.Length)]}",
            "video" => "🎬 فيديو",
            "audio" => "🎤 رسالة صوتية",
            _ => body.Length > 80 ? body[..80] + "…" : body
        };
        var recipients = await GetRecipientUserIdsAsync(appt, ct);
        foreach (var rid in recipients)
        {
            if (rid == userId) continue;
            var link = NotificationLinkForRecipient(rid, appt);
            await _notifications.NotifyAsync(rid, "رسالة جديدة في الموعد", preview, link, ct);
        }

        return (true, null);
    }

    private static string ClassifyAttachment(string url)
    {
        var u = url.ToLowerInvariant();
        if (u.EndsWith(".mp4") || u.EndsWith(".mov"))
            return "video";
        if (IsAudioAttachment(u))
            return "audio";
        return "image";
    }

    private static bool IsAudioAttachment(string uLower)
    {
        return uLower.EndsWith(".mp3") || uLower.EndsWith(".m4a") || uLower.EndsWith(".aac") || uLower.EndsWith(".ogg")
            || uLower.EndsWith(".wav") || uLower.EndsWith(".webm");
    }

    private static string NotificationLinkForRecipient(string recipientUserId, Appointment appt)
    {
        if (recipientUserId == appt.PatientId)
            return $"/patient/appointments/{appt.AppointmentId}";
        if (appt.NurseProfile != null && appt.NurseProfile.UserId == recipientUserId)
            return $"/nurse/appointments/{appt.AppointmentId}";
        if (appt.Clinic != null && appt.Clinic.OwnerId == recipientUserId)
            return $"/clinic/appointments/{appt.AppointmentId}";
        return $"/patient/appointments/{appt.AppointmentId}";
    }

    private async Task<List<string>> GetRecipientUserIdsAsync(Appointment appt, CancellationToken ct)
    {
        var ids = new List<string> { appt.PatientId };
        if (appt.NurseProfile != null)
            ids.Add(appt.NurseProfile.UserId);
        if (appt.Clinic != null)
            ids.Add(appt.Clinic.OwnerId);
        return await Task.FromResult(ids.Distinct().ToList());
    }

    private async Task<Appointment?> LoadAppointmentForParticipantAsync(int appointmentId, string userId, CancellationToken ct)
    {
        var appt = await _db.Appointments.AsNoTracking()
            .Include(a => a.NurseProfile)
            .Include(a => a.Clinic)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, ct);
        if (appt == null) return null;
        if (appt.PatientId == userId) return appt;
        if (appt.NurseProfile != null && appt.NurseProfile.UserId == userId) return appt;
        if (appt.Clinic != null && appt.Clinic.OwnerId == userId) return appt;
        return null;
    }
}
