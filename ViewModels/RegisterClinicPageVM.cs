using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.ViewModels;

public class RegisterClinicPageVM
{
    [Required(ErrorMessage = "اسم المالك مطلوب")]
    [Display(Name = "اسم المالك")]
    public string OwnerFullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "بريد المالك مطلوب")]
    [EmailAddress]
    [Display(Name = "بريد المالك")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "جوال المالك")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "ثمانية أحرف على الأقل")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "غير متطابقة مع كلمة المرور")]
    [Display(Name = "تأكيد كلمة المرور")]
    public string ConfirmPassword { get; set; } = string.Empty;

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

    [Display(Name = "شعار العيادة")]
    public IFormFile? LogoFile { get; set; }

    [Display(Name = "رخصة المنشأة أو التصريح الطبي / السجل التجاري")]
    public IFormFile? LicenseDocumentFile { get; set; }

    [Display(Name = "هاتف العيادة")]
    public string? ClinicPhone { get; set; }

    [EmailAddress]
    [Display(Name = "بريد العيادة")]
    public string? ClinicEmail { get; set; }
}
