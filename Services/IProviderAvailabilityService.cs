namespace HomeNursingSystem.Services;

public interface IProviderAvailabilityService
{
    Task<BookAvailabilityJson> GetPatientBookAvailabilityAsync(int? nurseProfileId, int? clinicId, CancellationToken ct = default);

    /// <summary>التحقق من أن الموعد (UTC) ضمن الجدول وغير محجوز.</summary>
    Task<bool> IsBookingAllowedAsync(bool isNurse, int nurseOrClinicId, DateTime appointmentUtc, CancellationToken ct = default);
}
