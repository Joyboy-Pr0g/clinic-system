using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.ViewModels;

public class ClinicProfileEditVM
{
    public int ClinicId { get; set; }

    [Required]
    [StringLength(150)]
    [Display(Name = "اسم العيادة")]
    public string ClinicName { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "الوصف")]
    public string? Description { get; set; }

    [Required]
    [StringLength(255)]
    [Display(Name = "العنوان")]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "الحي")]
    public string? Neighborhood { get; set; }

    [Display(Name = "المدينة")]
    public string? City { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    [Display(Name = "ساعات العمل")]
    public string? OpeningHours { get; set; }

    [Display(Name = "هاتف")]
    public string? PhoneNumber { get; set; }

    [EmailAddress]
    [Display(Name = "البريد")]
    public string? Email { get; set; }

    public string? LogoImagePath { get; set; }
    public string? CoverImagePath { get; set; }

    [Display(Name = "الشعار")]
    public IFormFile? LogoFile { get; set; }

    [Display(Name = "صورة الغلاف")]
    public IFormFile? CoverFile { get; set; }

    public string? GoogleMapsApiKey { get; set; }
}
