using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.ViewModels;

public class RatingVM
{
    public int AppointmentId { get; set; }

    [Display(Name = "التقييم")]
    public int Stars { get; set; }

    [StringLength(1000)]
    [Display(Name = "تعليق (اختياري)")]
    public string? Comment { get; set; }
}
