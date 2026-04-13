using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.ViewModels;

public class AppointmentBookVM : IValidatableObject
{
    public string BookType { get; set; } = "Nurse"; // Nurse | Clinic

    public int? NurseProfileId { get; set; }
    public int? ClinicId { get; set; }

    /// <summary>خدمة الممرض — من قائمة الممرض (NurseListingService).</summary>
    [Display(Name = "الخدمة")]
    public int? NurseListingServiceId { get; set; }

    /// <summary>خدمة العيادة من قائمة العيادة.</summary>
    [Display(Name = "خدمة العيادة")]
    public int? ClinicServiceId { get; set; }

    [Required(ErrorMessage = "التاريخ والوقت مطلوبان")]
    [Display(Name = "موعد الزيارة")]
    public DateTime AppointmentDate { get; set; }

    [Required(ErrorMessage = "العنوان مطلوب")]
    [StringLength(255)]
    [Display(Name = "العنوان النصي")]
    public string AddressText { get; set; } = string.Empty;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    [StringLength(500)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    public string? GoogleMapsApiKey { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AppointmentDate.Year < 2000)
            yield return new ValidationResult("اختر اليوم والساعة من الأوقات المتاحة.", new[] { nameof(AppointmentDate) });

        if (string.Equals(BookType, "Nurse", StringComparison.OrdinalIgnoreCase))
        {
            if (NurseListingServiceId is null or < 1)
                yield return new ValidationResult("اختر الخدمة.", new[] { nameof(NurseListingServiceId) });
        }
        else if (string.Equals(BookType, "Clinic", StringComparison.OrdinalIgnoreCase))
        {
            if (ClinicServiceId is null or < 1)
                yield return new ValidationResult("اختر خدمة العيادة.", new[] { nameof(ClinicServiceId) });
        }
    }
}
