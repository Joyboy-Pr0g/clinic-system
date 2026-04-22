namespace HomeNursingSystem.Services;

public interface IProviderAvailabilityService
{
    Task<BookAvailabilityJson> GetPatientBookAvailabilityAsync(int? nurseProfileId, int? clinicId, CancellationToken ct = default);

    /// <summary>التحقق من أن الموعد (UTC) ضمن الجدول وغير محجوز.</summary>
    Task<bool> IsBookingAllowedAsync(bool isNurse, int nurseOrClinicId, DateTime appointmentUtc, CancellationToken ct = default);

    /// <summary>قائمة الممرضين القادرين على استلام طلب فوري في الوقت الحالي.</summary>
    Task<HashSet<int>> GetCurrentlyAvailableNurseIdsAsync(CancellationToken ct = default);
}
