namespace HomeNursingSystem.Models;

public class Appointment
{
    public int AppointmentId { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public ApplicationUser Patient { get; set; } = null!;
    public int? NurseProfileId { get; set; }
    public NurseProfile? NurseProfile { get; set; }
    public int? ClinicId { get; set; }
    public Clinic? Clinic { get; set; }
    /// <summary>ممرض منزلي — خدمة من الكتالوج العام.</summary>
    public int? ServiceId { get; set; }
    public MedicalService? Service { get; set; }
    /// <summary>حجز عيادة — خدمة من قائمة العيادة.</summary>
    public int? ClinicServiceId { get; set; }
    public ClinicService? ClinicService { get; set; }
    /// <summary>حجز ممرض — خدمة من قائمة الممرض (اسم/سعر حر).</summary>
    public int? NurseListingServiceId { get; set; }
    public NurseListingService? NurseListingService { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string AddressText { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = AppointmentStatuses.Pending;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Rating? Rating { get; set; }
    public ICollection<AppointmentMessage> Messages { get; set; } = new List<AppointmentMessage>();
}
