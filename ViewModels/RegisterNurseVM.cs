using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.ViewModels;

public class RegisterNurseVM
{
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
}
