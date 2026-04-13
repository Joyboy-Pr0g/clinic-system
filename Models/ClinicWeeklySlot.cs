namespace HomeNursingSystem.Models;

/// <summary>أوقات متكررة أسبوعياً للعيادة (يوم + نافذة زمنية).</summary>
public class ClinicWeeklySlot
{
    public int ClinicWeeklySlotId { get; set; }
    public int ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
