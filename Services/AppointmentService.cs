using HomeNursingSystem.Data;
using HomeNursingSystem.Data.Repositories;
using HomeNursingSystem.Models;
using HomeNursingSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointments;
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notifications;

    public AppointmentService(
        IAppointmentRepository appointments,
        ApplicationDbContext db,
        INotificationService notifications)
    {
        _appointments = appointments;
        _db = db;
        _notifications = notifications;
    }

    public async Task<PatientDashboardVM> GetPatientDashboardAsync(string patientId, CancellationToken ct = default)
    {
        var q = _db.Appointments.Where(a => a.PatientId == patientId);
        return new PatientDashboardVM
        {
            TotalBookings = await q.CountAsync(ct),
            PendingApproval = await q.CountAsync(a => a.Status == AppointmentStatuses.Pending, ct),
            ActiveConfirmed = await q.CountAsync(a =>
                a.Status == AppointmentStatuses.Approved
                || a.Status == AppointmentStatuses.Confirmed
                || a.Status == AppointmentStatuses.InProgress, ct),
            Completed = await q.CountAsync(a => a.Status == AppointmentStatuses.Completed, ct),
            Cancelled = await q.CountAsync(a => a.Status == AppointmentStatuses.Cancelled, ct),
            RecentAppointments = await q
                .Include(a => a.NurseProfile)!.ThenInclude(n => n!.User)
                .Include(a => a.Clinic)
                .Include(a => a.Service)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(8)
                .Select(a => new AppointmentRowVM
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    ProviderName = a.NurseProfile != null
                        ? a.NurseProfile.User.FullName
                        : (a.Clinic != null ? a.Clinic.ClinicName : ""),
                    ServiceName = a.Service.ServiceName,
                    Status = a.Status,
                    TotalPrice = a.TotalPrice
                })
                .ToListAsync(ct)
        };
    }

    public async Task<NurseDashboardVM> GetNurseDashboardAsync(int nurseProfileId, CancellationToken ct = default)
    {
        var q = _db.Appointments.Where(a => a.NurseProfileId == nurseProfileId);
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var nurse = await _db.NurseProfiles.AsNoTracking().FirstAsync(n => n.NurseProfileId == nurseProfileId, ct);
        var pendingList = await q
            .Include(a => a.Patient)
            .Include(a => a.Service)
            .Where(a => a.Status == AppointmentStatuses.Pending)
            .OrderBy(a => a.AppointmentDate)
            .Take(25)
            .Select(a => new AppointmentRowVM
            {
                AppointmentId = a.AppointmentId,
                AppointmentDate = a.AppointmentDate,
                ProviderName = a.Patient.FullName,
                ServiceName = a.Service.ServiceName,
                Status = a.Status,
                TotalPrice = a.TotalPrice
            })
            .ToListAsync(ct);
        return new NurseDashboardVM
        {
            TodayCount = await q.CountAsync(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow && a.Status != AppointmentStatuses.Cancelled, ct),
            TotalCompleted = await q.CountAsync(a => a.Status == AppointmentStatuses.Completed, ct),
            AverageRating = nurse.AverageRating,
            PendingRequests = await q.CountAsync(a => a.Status == AppointmentStatuses.Pending, ct),
            PendingAppointments = pendingList,
            Upcoming = await q
                .Include(a => a.Patient)
                .Include(a => a.Service)
                .Where(a => a.AppointmentDate >= DateTime.UtcNow && a.Status != AppointmentStatuses.Cancelled && a.Status != AppointmentStatuses.Completed)
                .OrderBy(a => a.AppointmentDate)
                .Take(10)
                .Select(a => new AppointmentRowVM
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    ProviderName = a.Patient.FullName,
                    ServiceName = a.Service.ServiceName,
                    Status = a.Status,
                    TotalPrice = a.TotalPrice
                })
                .ToListAsync(ct)
        };
    }

    public async Task<ClinicDashboardVM> GetClinicDashboardAsync(int clinicId, CancellationToken ct = default)
    {
        var q = _db.Appointments.Where(a => a.ClinicId == clinicId);
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var clinic = await _db.Clinics.AsNoTracking().FirstAsync(c => c.ClinicId == clinicId, ct);
        return new ClinicDashboardVM
        {
            TodayCount = await q.CountAsync(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow && a.Status != AppointmentStatuses.Cancelled, ct),
            MonthlyTotal = await q.CountAsync(a => a.AppointmentDate >= monthStart && a.Status != AppointmentStatuses.Cancelled, ct),
            PendingRequests = await q.CountAsync(a => a.Status == AppointmentStatuses.Pending, ct),
            AverageRating = clinic.AverageRating,
            StaffCount = 1,
            Upcoming = await q
                .Include(a => a.Patient)
                .Include(a => a.Service)
                .Where(a => a.AppointmentDate >= DateTime.UtcNow && a.Status != AppointmentStatuses.Cancelled && a.Status != AppointmentStatuses.Completed)
                .OrderBy(a => a.AppointmentDate)
                .Take(10)
                .Select(a => new AppointmentRowVM
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    ProviderName = a.Patient.FullName,
                    ServiceName = a.Service.ServiceName,
                    Status = a.Status,
                    TotalPrice = a.TotalPrice
                })
                .ToListAsync(ct)
        };
    }

    public async Task<AdminDashboardVM> GetAdminDashboardAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var pendingVer = await _db.NurseProfiles.CountAsync(n => !n.IsVerified, ct)
            + await _db.Clinics.CountAsync(c => !c.IsVerified, ct);

        var recentAppts = await _db.Appointments
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .Select(a => new ActivityFeedItemVM
            {
                Text = $"موعد جديد #{a.AppointmentId} — {a.Status}",
                At = a.CreatedAt
            })
            .ToListAsync(ct);

        return new AdminDashboardVM
        {
            TotalUsers = await _db.Users.CountAsync(ct),
            TotalNurses = await _db.NurseProfiles.CountAsync(ct),
            TotalClinics = await _db.Clinics.CountAsync(ct),
            AppointmentsToday = await _db.Appointments.CountAsync(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow, ct),
            RevenueEstimate = await _db.Appointments
                .Where(a => a.Status == AppointmentStatuses.Completed && a.UpdatedAt >= monthStart)
                .SumAsync(a => (decimal?)a.TotalPrice, ct) ?? 0,
            PendingVerifications = pendingVer,
            RecentActivity = recentAppts
        };
    }

    public async Task<(bool ok, string? error, int? appointmentId)> BookAsync(AppointmentBookVM model, string patientId, CancellationToken ct = default)
    {
        MedicalService? svc = await _db.Services.FirstOrDefaultAsync(s => s.ServiceId == model.ServiceId && s.IsActive, ct);
        if (svc == null)
            return (false, "الخدمة غير متاحة.", null);

        decimal price;
        int? nurseId = null;
        int? clinicId = null;

        if (model.BookType.Equals("Nurse", StringComparison.OrdinalIgnoreCase))
        {
            if (!model.NurseProfileId.HasValue)
                return (false, "اختر الممرض.", null);
            var nurse = await _db.NurseProfiles
                .Include(n => n.NurseServices)
                .FirstOrDefaultAsync(n => n.NurseProfileId == model.NurseProfileId && n.IsVerified && n.IsAvailable, ct);
            if (nurse == null)
                return (false, "الممرض غير متاح.", null);
            var ns = nurse.NurseServices.FirstOrDefault(x => x.ServiceId == model.ServiceId);
            price = ns?.CustomPrice ?? svc.BasePrice;
            nurseId = nurse.NurseProfileId;
        }
        else if (model.BookType.Equals("Clinic", StringComparison.OrdinalIgnoreCase))
        {
            if (!model.ClinicId.HasValue)
                return (false, "اختر العيادة.", null);
            var clinic = await _db.Clinics.FirstOrDefaultAsync(c => c.ClinicId == model.ClinicId && c.IsVerified && c.IsActive, ct);
            if (clinic == null)
                return (false, "العيادة غير متاحة.", null);
            price = svc.BasePrice;
            clinicId = clinic.ClinicId;
        }
        else
            return (false, "نوع الحجز غير صالح.", null);

        var appt = new Appointment
        {
            PatientId = patientId,
            NurseProfileId = nurseId,
            ClinicId = clinicId,
            ServiceId = model.ServiceId,
            AppointmentDate = model.AppointmentDate.ToUniversalTime(),
            AddressText = model.AddressText,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            Notes = model.Notes,
            Status = AppointmentStatuses.Pending,
            TotalPrice = price,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _appointments.AddAsync(appt, ct);
        await _appointments.SaveChangesAsync(ct);

        if (nurseId.HasValue)
        {
            var uid = await _db.NurseProfiles.Where(n => n.NurseProfileId == nurseId).Select(n => n.UserId).FirstAsync(ct);
            await _notifications.NotifyAsync(uid, "طلب موعد جديد", "لديك طلب موعد بانتظار التأكيد.", $"/nurse/appointments/{appt.AppointmentId}", ct);
        }
        else if (clinicId.HasValue)
        {
            var ownerId = await _db.Clinics.Where(c => c.ClinicId == clinicId).Select(c => c.OwnerId).FirstAsync(ct);
            await _notifications.NotifyAsync(ownerId, "طلب موعد للعيادة", "موعد جديد بانتظار التأكيد.", $"/clinic/appointments/{appt.AppointmentId}", ct);
        }

        return (true, null, appt.AppointmentId);
    }

    public async Task<(bool ok, string? error)> UpdateStatusAsync(int appointmentId, string status, string actorUserId, string role, CancellationToken ct = default)
    {
        var appt = await _appointments.GetByIdAsync(appointmentId, ct);
        if (appt == null)
            return (false, "الموعد غير موجود.");

        if (role == AppRoles.Nurse)
        {
            var np = await _db.NurseProfiles.AsNoTracking().FirstOrDefaultAsync(n => n.UserId == actorUserId, ct);
            if (np == null || appt.NurseProfileId != np.NurseProfileId)
                return (false, "غير مصرح.");
        }
        else if (role == AppRoles.ClinicOwner)
        {
            var c = await _db.Clinics.AsNoTracking().FirstOrDefaultAsync(x => x.OwnerId == actorUserId, ct);
            if (c == null || appt.ClinicId != c.ClinicId)
                return (false, "غير مصرح.");
        }
        else if (role != AppRoles.Admin)
            return (false, "غير مصرح.");

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            AppointmentStatuses.Pending, AppointmentStatuses.Approved, AppointmentStatuses.Confirmed,
            AppointmentStatuses.InProgress, AppointmentStatuses.Completed, AppointmentStatuses.Cancelled
        };
        if (!allowed.Contains(status))
            return (false, "حالة غير صالحة.");

        var previousStatus = appt.Status;
        appt.Status = status;
        appt.UpdatedAt = DateTime.UtcNow;
        _appointments.Update(appt);
        await _appointments.SaveChangesAsync(ct);

        var title = "تحديث الموعد";
        var msg = $"حالة موعدك #{appointmentId}: {status}";
        if (AppointmentStatuses.IsApproved(status) && previousStatus == AppointmentStatuses.Pending)
        {
            title = "تم قبول موعدك";
            msg = "يمكنك متابعة التواصل عبر الدردشة داخل صفحة الموعد.";
        }

        await _notifications.NotifyAsync(appt.PatientId, title, msg, $"/patient/appointments/{appointmentId}", ct);

        return (true, null);
    }
}
