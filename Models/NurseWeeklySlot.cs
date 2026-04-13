namespace HomeNursingSystem.Models;

/// <summary>أوقات متكررة أسبوعياً للممرض (يوم + نافذة زمنية).</summary>
public class NurseWeeklySlot
{
    public int NurseWeeklySlotId { get; set; }
    public int NurseProfileId { get; set; }
    public NurseProfile NurseProfile { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
