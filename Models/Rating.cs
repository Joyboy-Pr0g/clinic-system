namespace HomeNursingSystem.Models;

public class Rating
{
    public int RatingId { get; set; }
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public string PatientId { get; set; } = string.Empty;
    public ApplicationUser Patient { get; set; } = null!;
    public string TargetType { get; set; } = RatingTargetTypes.Nurse;
    public int? NurseProfileId { get; set; }
    public NurseProfile? NurseProfile { get; set; }
    public int? ClinicId { get; set; }
    public Clinic? Clinic { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public bool IsApproved { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
