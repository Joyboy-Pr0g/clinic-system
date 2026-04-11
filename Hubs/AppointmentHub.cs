using System.Security.Claims;
using HomeNursingSystem.Data;
using HomeNursingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Hubs;

[Authorize]
public class AppointmentHub : Hub
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AppointmentHub(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public static string GroupName(int appointmentId) => $"appt-{appointmentId}";

    public async Task JoinAppointment(int appointmentId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new HubException("يجب تسجيل الدخول.");

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!await IsParticipantAsync(db, appointmentId, userId, Context.ConnectionAborted))
            throw new HubException("لا تملك صلاحية هذا الموعد.");

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(appointmentId));
    }

    public async Task PushNurseLocation(int appointmentId, double latitude, double longitude)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new HubException("يجب تسجيل الدخول.");

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var appt = await db.Appointments
            .Include(a => a.NurseProfile)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, Context.ConnectionAborted);
        if (appt == null || appt.NurseProfileId == null || appt.NurseProfile?.UserId != userId)
            throw new HubException("غير مصرح.");
        if (!AppointmentStatuses.IsApproved(appt.Status) && !string.Equals(appt.Status, AppointmentStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
            throw new HubException("التتبع غير مفعّل لهذه الحالة.");

        var nurse = await db.NurseProfiles.FirstAsync(n => n.NurseProfileId == appt.NurseProfileId.Value, Context.ConnectionAborted);
        nurse.LastLatitude = latitude;
        nurse.LastLongitude = longitude;
        nurse.LocationUpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(Context.ConnectionAborted);

        await Clients.OthersInGroup(GroupName(appointmentId)).SendAsync("NurseLocationUpdated", latitude, longitude, cancellationToken: Context.ConnectionAborted);
    }

    public async Task PushPatientLocation(int appointmentId, double latitude, double longitude)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new HubException("يجب تسجيل الدخول.");

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var appt = await db.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, Context.ConnectionAborted);
        if (appt == null || appt.PatientId != userId)
            throw new HubException("غير مصرح.");
        if (!AppointmentStatuses.IsApproved(appt.Status) && !string.Equals(appt.Status, AppointmentStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
            throw new HubException("التتبع غير مفعّل لهذه الحالة.");

        var patient = await db.Users.FirstAsync(u => u.Id == userId, Context.ConnectionAborted);
        patient.Latitude = latitude;
        patient.Longitude = longitude;
        patient.LastLiveLocationAt = DateTime.UtcNow;
        await db.SaveChangesAsync(Context.ConnectionAborted);

        await Clients.OthersInGroup(GroupName(appointmentId)).SendAsync("PatientLocationUpdated", latitude, longitude, cancellationToken: Context.ConnectionAborted);
    }

    public static async Task<bool> IsParticipantAsync(ApplicationDbContext db, int appointmentId, string userId, CancellationToken ct = default)
    {
        var a = await db.Appointments.AsNoTracking()
            .Include(x => x.NurseProfile)
            .Include(x => x.Clinic)
            .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId, ct);
        if (a == null) return false;
        if (a.PatientId == userId) return true;
        if (a.NurseProfile != null && a.NurseProfile.UserId == userId) return true;
        if (a.Clinic != null && a.Clinic.OwnerId == userId) return true;
        return false;
    }
}
