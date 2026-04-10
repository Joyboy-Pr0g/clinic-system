using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.ViewModels;

public class ContactVM
{
    [Required(ErrorMessage = "الاسم مطلوب")]
    [StringLength(100)]
    [Display(Name = "الاسم")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "البريد")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "الجوال")]
    public string? PhoneNumber { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "الموضوع")]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    [Display(Name = "الرسالة")]
    public string Message { get; set; } = string.Empty;
}
