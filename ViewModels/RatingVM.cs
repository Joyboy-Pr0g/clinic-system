using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.ViewModels;

public class RatingVM
{
    public int AppointmentId { get; set; }

    [Range(1, 5)]
    [Display(Name = "التقييم")]
    public int Stars { get; set; } = 5;

    [StringLength(1000)]
    [Display(Name = "تعليق (اختياري)")]
    public string? Comment { get; set; }
}
