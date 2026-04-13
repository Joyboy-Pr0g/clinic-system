using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.ViewModels;

public class ServiceEditVM
{
    public int ServiceId { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "الاسم")]
    public string ServiceName { get; set; } = string.Empty;

    [StringLength(2000)]
    [Display(Name = "الوصف")]
    public string? Description { get; set; }

    [Range(0, 1000000)]
    [Display(Name = "السعر")]
    public decimal BasePrice { get; set; }

    [StringLength(120)]
    [Display(Name = "أيقونة Font Awesome")]
    public string? IconClass { get; set; }
}
