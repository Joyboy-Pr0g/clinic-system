using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.ViewModels;

public class RegisterNursePageVM
{
    [Required(ErrorMessage = "الاسم مطلوب")]
    [StringLength(100)]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "البريد")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "الجوال")]
    public string? PhoneNumber { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "تأكيد كلمة المرور")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Display(Name = "التخصص")]
    public string Specialization { get; set; } = string.Empty;

    [Range(0, 60)]
    [Display(Name = "سنوات الخبرة")]
    public int YearsOfExperience { get; set; }

    [StringLength(500)]
    [Display(Name = "نبذة")]
    public string? Bio { get; set; }

    [Display(Name = "صورة الرخصة")]
    public IFormFile? LicenseFile { get; set; }

    [Display(Name = "المدينة")]
    public string? City { get; set; }

    [Display(Name = "الحي")]
    public string? Neighborhood { get; set; }
}
