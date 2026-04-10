using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.ViewModels;

public class AppointmentBookVM
{
    public string BookType { get; set; } = "Nurse"; // Nurse | Clinic

    public int? NurseProfileId { get; set; }
    public int? ClinicId { get; set; }

    [Required(ErrorMessage = "اختر الخدمة")]
    [Display(Name = "الخدمة")]
    public int ServiceId { get; set; }

    [Required(ErrorMessage = "التاريخ والوقت مطلوبان")]
    [Display(Name = "موعد الزيارة")]
    public DateTime AppointmentDate { get; set; } = DateTime.Now.AddDays(1);

    [Required(ErrorMessage = "العنوان مطلوب")]
    [StringLength(255)]
    [Display(Name = "العنوان النصي")]
    public string AddressText { get; set; } = string.Empty;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    [StringLength(500)]
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    public string? GoogleMapsApiKey { get; set; }
}
