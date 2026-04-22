using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.ViewModels;

public class ClinicServiceEditVM
{
    public int? ClinicServiceId { get; set; }

    [Required(ErrorMessage = "أدخل اسم الخدمة")]
    [StringLength(200, ErrorMessage = "الاسم طويل جداً")]
    [Display(Name = "اسم الخدمة")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0, 999999.99, ErrorMessage = "السعر غير صالح")]
    [Display(Name = "السعر (ر.ي)")]
    public decimal Price { get; set; }
}
