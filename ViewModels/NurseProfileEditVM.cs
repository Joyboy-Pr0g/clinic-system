using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.ViewModels;

public class NurseProfileEditVM
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

    [Display(Name = "متاح لاستقبال الطلبات")]
    public bool IsAvailable { get; set; } = true;

    public List<NurseServiceEditRowVM> ServiceRows { get; set; } = new();

    [Display(Name = "صورة شخصية (اختياري)")]
    public IFormFile? ProfileImage { get; set; }

    public string? CurrentProfileImagePath { get; set; }
}

public class NurseServiceEditRowVM
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public bool Selected { get; set; }
    public decimal CustomPrice { get; set; }
}
