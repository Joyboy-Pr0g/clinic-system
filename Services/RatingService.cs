using HomeNursingSystem.Data;
using HomeNursingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Services;

public class RatingService : IRatingService
{
    private readonly ApplicationDbContext _db;

    public RatingService(ApplicationDbContext db) => _db = db;

    public async Task<(bool ok, string? error)> SubmitRatingAsync(string patientId, int appointmentId, int stars, string? comment, CancellationToken ct = default)
    {
        if (stars is < 1 or > 5)
            return (false, "عدد النجوم غير صالح.");

        var appt = await _db.Appointments
            .Include(a => a.Rating)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.PatientId == patientId, ct);

        if (appt == null)
            return (false, "الموعد غير موجود.");
        if (appt.Status != AppointmentStatuses.Completed)
            return (false, "يمكن التقييم بعد اكتمال الخدمة فقط.");
        if (appt.Rating != null)
            return (false, "تم التقييم مسبقاً.");

        string targetType;
        int? nurseId = appt.NurseProfileId;
        int? clinicId = appt.ClinicId;
        if (nurseId.HasValue)
            targetType = RatingTargetTypes.Nurse;
        else if (clinicId.HasValue)
            targetType = RatingTargetTypes.Clinic;
        else
            return (false, "لا يوجد هدف للتقييم.");

        var rating = new Rating
        {
            AppointmentId = appointmentId,
            PatientId = patientId,
            TargetType = targetType,
            NurseProfileId = nurseId,
            ClinicId = clinicId,
            Stars = stars,
            Comment = comment,
            IsApproved = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.Ratings.Add(rating);
        await _db.SaveChangesAsync(ct);

        if (nurseId.HasValue)
            await RecalculateNurseAsync(nurseId.Value, ct);
        else if (clinicId.HasValue)
            await RecalculateClinicAsync(clinicId.Value, ct);

        return (true, null);
    }

    private async Task RecalculateNurseAsync(int nurseProfileId, CancellationToken ct)
    {
        var q = _db.Ratings.Where(r => r.NurseProfileId == nurseProfileId && r.IsApproved);
        var avg = await q.AverageAsync(r => (double?)r.Stars, ct) ?? 0;
        var count = await q.CountAsync(ct);
        await _db.NurseProfiles.Where(n => n.NurseProfileId == nurseProfileId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.AverageRating, (decimal)Math.Round(avg, 2))
                .SetProperty(n => n.TotalReviews, count), ct);
    }

    private async Task RecalculateClinicAsync(int clinicId, CancellationToken ct)
    {
        var q = _db.Ratings.Where(r => r.ClinicId == clinicId && r.IsApproved);
        var avg = await q.AverageAsync(r => (double?)r.Stars, ct) ?? 0;
        var count = await q.CountAsync(ct);
        await _db.Clinics.Where(c => c.ClinicId == clinicId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.AverageRating, (decimal)Math.Round(avg, 2))
                .SetProperty(c => c.TotalReviews, count), ct);
    }
}
