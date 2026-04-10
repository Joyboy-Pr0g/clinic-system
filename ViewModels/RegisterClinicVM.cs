using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.ViewModels;

public class RegisterClinicVM
{
    [Required(ErrorMessage = "اسم العيادة مطلوب")]
    [StringLength(150)]
    [Display(Name = "اسم العيادة")]
    public string ClinicName { get; set; } = string.Empty;

    [Required(ErrorMessage = "العنوان مطلوب")]
    [StringLength(255)]
    [Display(Name = "العنوان")]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "الحي")]
    public string? Neighborhood { get; set; }

    [Display(Name = "المدينة")]
    public string? City { get; set; }

    [Display(Name = "الشعار")]
    public IFormFile? LogoFile { get; set; }

    [Display(Name = "هاتف العيادة")]
    public string? PhoneNumber { get; set; }

    [EmailAddress]
    [Display(Name = "بريد العيادة")]
    public string? Email { get; set; }
}
