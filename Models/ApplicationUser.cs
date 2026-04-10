using Microsoft.AspNetCore.Identity;

namespace HomeNursingSystem.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImagePath { get; set; }
    public string? City { get; set; }
    public string? Neighborhood { get; set; }
    public string? Street { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Role { get; set; } = AppRoles.Patient;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public NurseProfile? NurseProfile { get; set; }
    public ICollection<Clinic> OwnedClinics { get; set; } = new List<Clinic>();
    public ICollection<Appointment> PatientAppointments { get; set; } = new List<Appointment>();
    public ICollection<Rating> RatingsGiven { get; set; } = new List<Rating>();
    public ICollection<Article> Articles { get; set; } = new List<Article>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
