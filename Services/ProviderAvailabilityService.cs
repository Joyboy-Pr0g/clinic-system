using HomeNursingSystem.Data;
using HomeNursingSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HomeNursingSystem.Services;

public class ProviderAvailabilityService : IProviderAvailabilityService
{
    private readonly ApplicationDbContext _db;

    public const int SlotMinutes = 60;
    public const int HorizonDays = 7;
    private static readonly TimeSpan MinLeadTime = TimeSpan.FromMinutes(15);

    public ProviderAvailabilityService(ApplicationDbContext db) => _db = db;

    private static TimeZoneInfo AppTimeZone =>
        TimeZoneInfo.TryFindSystemTimeZoneById("Asia/Riyadh", out var riyadh) ? riyadh : TimeZoneInfo.TryFindSystemTimeZoneById("Arab Standard Time", out var arab) ? arab
        : TimeZoneInfo.Local;

    public async Task<BookAvailabilityJson> GetPatientBookAvailabilityAsync(int? nurseProfileId, int? clinicId, CancellationToken ct = default)
    {
        var tz = AppTimeZone;
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        var todayLocal = nowLocal.Date;

        var nurseWindows = nurseProfileId is > 0
            ? await _db.NurseWeeklySlots.AsNoTracking()
                .Where(s => s.NurseProfileId == nurseProfileId)
                .Select(s => new { s.DayOfWeek, s.StartTime, s.EndTime })
                .ToListAsync(ct)
            : null;
        var clinicWindows = clinicId is > 0
            ? await _db.ClinicWeeklySlots.AsNoTracking()
                .Where(s => s.ClinicId == clinicId)
                .Select(s => new { s.DayOfWeek, s.StartTime, s.EndTime })
                .ToListAsync(ct)
            : null;
        if (nurseWindows == null && clinicWindows == null)
            return new BookAvailabilityJson { SlotMinutes = SlotMinutes, Days = new List<BookAvailabilityDayJson>() };

        var blockedStartsUtc = await GetBlockedSlotStartsUtcAsync(nurseProfileId, clinicId, todayLocal, tz, ct);

        var ar = CultureInfo.GetCultureInfo("ar-SA");
        var days = new List<BookAvailabilityDayJson>();
        for (var i = 0; i < HorizonDays; i++)
        {
            var dateLocal = todayLocal.AddDays(i);
            var dow = dateLocal.DayOfWeek;
            var dayWindows = nurseWindows != null
                ? nurseWindows.Where(w => w.DayOfWeek == dow).ToList()
                : clinicWindows!.Where(w => w.DayOfWeek == dow).ToList();
            var slots = new List<BookAvailabilitySlotJson>();
            foreach (var w in dayWindows)
            {
                if (w.EndTime <= w.StartTime) continue;
                for (var t = dateLocal + w.StartTime; t + TimeSpan.FromMinutes(SlotMinutes) <= dateLocal + w.EndTime; t = t.AddMinutes(SlotMinutes))
                {
                    var trimmed = new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, 0, DateTimeKind.Unspecified);
                    var utc = NormalizeSlotUtc(TimeZoneInfo.ConvertTimeToUtc(trimmed, tz));
                    if (utc < DateTime.UtcNow + MinLeadTime) continue;
                    if (blockedStartsUtc.Contains(utc)) continue;
                    slots.Add(new BookAvailabilitySlotJson
                    {
                        UtcIso = utc.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture),
                        TimeLabel = t.ToString("HH:mm", CultureInfo.InvariantCulture)
                    });
                }
            }

            days.Add(new BookAvailabilityDayJson
            {
                Date = dateLocal.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                WeekdayLabel = dateLocal.ToString("dddd d MMMM", ar),
                Slots = slots
            });
        }

        return new BookAvailabilityJson { SlotMinutes = SlotMinutes, Days = days };
    }

    private async Task<HashSet<DateTime>> GetBlockedSlotStartsUtcAsync(int? nurseProfileId, int? clinicId, DateTime todayLocal, TimeZoneInfo tz, CancellationToken ct)
    {
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(todayLocal, DateTimeKind.Unspecified), tz);
        var endLocal = todayLocal.AddDays(HorizonDays + 1);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(endLocal, DateTimeKind.Unspecified), tz);

        IQueryable<Appointment> q = _db.Appointments.AsNoTracking()
            .Where(a => a.AppointmentDate >= startUtc && a.AppointmentDate < endUtc
                && a.Status != AppointmentStatuses.Cancelled);

        if (nurseProfileId is > 0)
            q = q.Where(a => a.NurseProfileId == nurseProfileId);
        else
            q = q.Where(a => a.ClinicId == clinicId);

        var times = await q.Select(a => a.AppointmentDate).ToListAsync(ct);
        return times.Select(NormalizeSlotUtc).ToHashSet();
    }

    private static DateTime NormalizeSlotUtc(DateTime utc) =>
        new(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, 0, DateTimeKind.Utc);

    public async Task<bool> IsBookingAllowedAsync(bool isNurse, int nurseOrClinicId, DateTime appointmentUtc, CancellationToken ct = default)
    {
        if (appointmentUtc.Kind != DateTimeKind.Utc)
            appointmentUtc = appointmentUtc.ToUniversalTime();
        appointmentUtc = NormalizeSlotUtc(appointmentUtc);
        if (appointmentUtc < DateTime.UtcNow + MinLeadTime)
            return false;

        var tz = AppTimeZone;
        var local = TimeZoneInfo.ConvertTimeFromUtc(appointmentUtc, tz);
        var dateLocal = local.Date;
        var timeOfDay = local.TimeOfDay;

        List<(TimeSpan Start, TimeSpan End)> windows;
        if (isNurse)
        {
            windows = await _db.NurseWeeklySlots.AsNoTracking()
                .Where(s => s.NurseProfileId == nurseOrClinicId && s.DayOfWeek == local.DayOfWeek)
                .Select(s => new ValueTuple<TimeSpan, TimeSpan>(s.StartTime, s.EndTime))
                .ToListAsync(ct);
        }
        else
        {
            windows = await _db.ClinicWeeklySlots.AsNoTracking()
                .Where(s => s.ClinicId == nurseOrClinicId && s.DayOfWeek == local.DayOfWeek)
                .Select(s => new ValueTuple<TimeSpan, TimeSpan>(s.StartTime, s.EndTime))
                .ToListAsync(ct);
        }

        if (windows.Count == 0) return false;

        TimeSpan? matchedStart = null;
        foreach (var w in windows)
        {
            if (timeOfDay >= w.Start && timeOfDay + TimeSpan.FromMinutes(SlotMinutes) <= w.End)
            {
                matchedStart = w.Start;
                break;
            }
        }
        if (matchedStart == null) return false;

        var offsetFromWindow = (int)(timeOfDay - matchedStart.Value).TotalMinutes;
        if (offsetFromWindow < 0 || offsetFromWindow % SlotMinutes != 0) return false;

        var nearby = await _db.Appointments.AsNoTracking()
            .Where(a =>
                a.Status != AppointmentStatuses.Cancelled
                && (isNurse ? a.NurseProfileId == nurseOrClinicId : a.ClinicId == nurseOrClinicId)
                && a.AppointmentDate >= appointmentUtc.AddHours(-12)
                && a.AppointmentDate <= appointmentUtc.AddHours(12))
            .Select(a => a.AppointmentDate)
            .ToListAsync(ct);

        return !nearby.Any(t => NormalizeSlotUtc(t.ToUniversalTime()) == appointmentUtc);
    }
}
