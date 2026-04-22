using System.ComponentModel.DataAnnotations;
using HomeNursingSystem.Models;

namespace HomeNursingSystem.ViewModels;

public class RegisterVM
{
    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [StringLength(100)]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد مطلوب")]
    [EmailAddress]
    [Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "رقم الجوال")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "ثمانية أحرف على الأقل")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "تأكيد كلمة المرور")]
    [Compare(nameof(Password), ErrorMessage = "غير متطابقة مع كلمة المرور")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "نوع الحساب مطلوب")]
    [Display(Name = "نوع الحساب")]
    public string SelectedRole { get; set; } = AppRoles.Patient;

    [Display(Name = "المدينة")]
    public string? City { get; set; }

    [Display(Name = "الحي")]
    public string? Neighborhood { get; set; }

    [Display(Name = "الشارع")]
    public string? Street { get; set; }

    [Display(Name = "الصورة الشخصية (اختياري)")]
    public IFormFile? ProfileImage { get; set; }
}
