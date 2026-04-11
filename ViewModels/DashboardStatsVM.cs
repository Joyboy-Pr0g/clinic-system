namespace HomeNursingSystem.ViewModels;

public class PatientDashboardVM
{
    public int TotalBookings { get; set; }
    /// <summary>بانتظار قبول الممرض/العيادة.</summary>
    public int PendingApproval { get; set; }
    /// <summary>مقبولة أو قيد التنفيذ (تشمل القيمة القديمة Confirmed).</summary>
    public int ActiveConfirmed { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public IReadOnlyList<AppointmentRowVM> RecentAppointments { get; set; } = Array.Empty<AppointmentRowVM>();
}

public class AppointmentRowVM
{
    public int AppointmentId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
}

public class NurseDashboardVM
{
    public int TodayCount { get; set; }
    public int TotalCompleted { get; set; }
    public decimal AverageRating { get; set; }
    public int PendingRequests { get; set; }
    public IReadOnlyList<AppointmentRowVM> PendingAppointments { get; set; } = Array.Empty<AppointmentRowVM>();
    public IReadOnlyList<AppointmentRowVM> Upcoming { get; set; } = Array.Empty<AppointmentRowVM>();
}

public class ClinicDashboardVM
{
    public int TodayCount { get; set; }
    public int MonthlyTotal { get; set; }
    public int PendingRequests { get; set; }
    public decimal AverageRating { get; set; }
    public int StaffCount { get; set; }
    public IReadOnlyList<AppointmentRowVM> Upcoming { get; set; } = Array.Empty<AppointmentRowVM>();
}

public class AdminDashboardVM
{
    public int TotalUsers { get; set; }
    public int TotalNurses { get; set; }
    public int TotalClinics { get; set; }
    public int AppointmentsToday { get; set; }
    public decimal RevenueEstimate { get; set; }
    public int PendingVerifications { get; set; }
    public IReadOnlyList<ActivityFeedItemVM> RecentActivity { get; set; } = Array.Empty<ActivityFeedItemVM>();
}

public class ActivityFeedItemVM
{
    public string Text { get; set; } = string.Empty;
    public DateTime At { get; set; }
}

public class NotificationItemVM
{
    public int NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NurseListItemVM
{
    public int NurseProfileId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImage { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsAvailable { get; set; }
    public string? Neighborhood { get; set; }
    public IReadOnlyList<string> ServiceNames { get; set; } = Array.Empty<string>();
}

public class NurseDetailsVM
{
    public int NurseProfileId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImage { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string? Bio { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsAvailable { get; set; }
    public IReadOnlyList<NurseServicePriceVM> Services { get; set; } = Array.Empty<NurseServicePriceVM>();
    public IReadOnlyList<RatingDisplayVM> Reviews { get; set; } = Array.Empty<RatingDisplayVM>();
}

public class NurseServicePriceVM
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class RatingDisplayVM
{
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ClinicListItemVM
{
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public string? LogoImagePath { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public decimal AverageRating { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class ClinicDetailsVM
{
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? OpeningHours { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? LogoImagePath { get; set; }
    public string? CoverImagePath { get; set; }
    public decimal AverageRating { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public IReadOnlyList<ServiceListItemVM> ServicesOffered { get; set; } = Array.Empty<ServiceListItemVM>();
    public IReadOnlyList<RatingDisplayVM> Reviews { get; set; } = Array.Empty<RatingDisplayVM>();
}

public class ServiceListItemVM
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
}

public class ArticleListItemVM
{
    public int ArticleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ThumbnailImagePath { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsNews { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class ArticleDetailsVM
{
    public int ArticleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ThumbnailImagePath { get; set; }
    public string Category { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public DateTime? PublishedAt { get; set; }
    public IReadOnlyList<ArticleListItemVM> Related { get; set; } = Array.Empty<ArticleListItemVM>();
}

public class LandingVM
{
    public IReadOnlyList<NurseListItemVM> FeaturedNurses { get; set; } = Array.Empty<NurseListItemVM>();
    public IReadOnlyList<ServiceListItemVM> Services { get; set; } = Array.Empty<ServiceListItemVM>();
    public int NursesCount { get; set; }
    public int ClinicsCount { get; set; }
    public int AppointmentsCount { get; set; }
}
