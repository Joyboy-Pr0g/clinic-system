using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.ViewModels;

public class RegisterClinicPageVM
{
    [Required]
    [Display(Name = "اسم المالك")]
    public string OwnerFullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "بريد المالك")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "جوال المالك")]
    public string? PhoneNumber { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    [Display(Name = "اسم العيادة")]
    public string ClinicName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Display(Name = "العنوان")]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "الحي")]
    public string? Neighborhood { get; set; }

    [Display(Name = "المدينة")]
    public string? City { get; set; }

    [Display(Name = "شعار العيادة")]
    public IFormFile? LogoFile { get; set; }

    [Display(Name = "هاتف العيادة")]
    public string? ClinicPhone { get; set; }

    [EmailAddress]
    [Display(Name = "بريد العيادة")]
    public string? ClinicEmail { get; set; }
}
