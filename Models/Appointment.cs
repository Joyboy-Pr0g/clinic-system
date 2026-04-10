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
    public int ServiceId { get; set; }
    public MedicalService Service { get; set; } = null!;
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
}
