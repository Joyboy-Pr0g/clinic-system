namespace HomeNursingSystem.Models;

public class NurseProfile
{
    public int NurseProfileId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public string Specialization { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string? LicenseImagePath { get; set; }
    public string? Bio { get; set; }
    public bool IsVerified { get; set; }
    /// <summary>رفض الأدمن لطلب التحقق — يُوجّه المستخدم لصفحة توضيحية.</summary>
    public bool IsRejectedByAdmin { get; set; }
    public string? AdminRejectionNote { get; set; }
    public bool IsAvailable { get; set; } = true;
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public double? LastLatitude { get; set; }
    public double? LastLongitude { get; set; }
    public DateTime? LocationUpdatedAt { get; set; }

    public ICollection<NurseServiceLink> NurseServices { get; set; } = new List<NurseServiceLink>();
    public ICollection<NurseListingService> NurseListingServices { get; set; } = new List<NurseListingService>();
    public ICollection<NurseWeeklySlot> WeeklySlots { get; set; } = new List<NurseWeeklySlot>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
