namespace HomeNursingSystem.Models;

public class Clinic
{
    public int ClinicId { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser Owner { get; set; } = null!;
    public string ClinicName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? LogoImagePath { get; set; }
    public string? CoverImagePath { get; set; }
    public string? OpeningHours { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
