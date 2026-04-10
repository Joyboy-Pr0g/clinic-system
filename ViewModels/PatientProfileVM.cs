using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.ViewModels;

public class PatientProfileVM
{
    [Required]
    [StringLength(100)]
    [Display(Name = "الاسم")]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "الجوال")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "المدينة")]
    public string? City { get; set; }

    [Display(Name = "الحي")]
    public string? Neighborhood { get; set; }

    [Display(Name = "الشارع")]
    public string? Street { get; set; }

    public string? ProfileImagePath { get; set; }

    [Display(Name = "صورة شخصية")]
    public IFormFile? ProfileImage { get; set; }
}
