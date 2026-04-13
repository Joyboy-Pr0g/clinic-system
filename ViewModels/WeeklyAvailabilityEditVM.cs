using System.ComponentModel.DataAnnotations;
using HomeNursingSystem.Models;

namespace HomeNursingSystem.ViewModels;

public class DailyAvailabilityRowVM
{
    public DayOfWeek DayOfWeek { get; set; }

    [Display(Name = "متاح")]
    public bool Enabled { get; set; }

    /// <summary>تنسيق HH:mm لحقل time.</summary>
    [Display(Name = "من")]
    public string? Start { get; set; }

    [Display(Name = "إلى")]
    public string? End { get; set; }
}

public class WeeklyAvailabilityEditVM
{
    public List<DailyAvailabilityRowVM> Days { get; set; } = new();

    public static WeeklyAvailabilityEditVM CreateDefault()
    {
        var order = WeekDisplayOrder;
        var list = new List<DailyAvailabilityRowVM>();
        foreach (var dow in order)
        {
            list.Add(new DailyAvailabilityRowVM
            {
                DayOfWeek = dow,
                Enabled = false,
                Start = "09:00",
                End = "17:00"
            });
        }
        return new WeeklyAvailabilityEditVM { Days = list };
    }

    /// <summary>ترتيب عرض أيام الأسبوع (يبدأ السبت).</summary>
    public static readonly DayOfWeek[] WeekDisplayOrder =
    {
        DayOfWeek.Saturday, DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday,
        DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday
    };
}
