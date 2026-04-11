namespace HomeNursingSystem.Models;

public static class AppointmentStatuses
{
    public const string Pending = "Pending";
    /// <summary>موعد مقبول من الممرض/العيادة (المعنى السابق لـ Confirmed).</summary>
    public const string Approved = "Approved";
    /// <summary>قيمة قديمة في قواعد بيانات موجودة — يُعامل مثل Approved.</summary>
    public const string Confirmed = "Confirmed";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static bool IsApproved(string? status) =>
        string.Equals(status, Approved, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, Confirmed, StringComparison.OrdinalIgnoreCase);

    public static bool ChatAndTrackingUnlocked(string? status) =>
        IsApproved(status) || string.Equals(status, InProgress, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, Completed, StringComparison.OrdinalIgnoreCase);

    /// <summary>اسم فئة الـ CSS للشارات (badge-*) في الجداول.</summary>
    public static string BadgeCssClass(string? status)
    {
        if (string.IsNullOrEmpty(status)) return "muted";
        if (string.Equals(status, InProgress, StringComparison.OrdinalIgnoreCase)) return "inprogress";
        if (IsApproved(status)) return "approved";
        return status.ToLowerInvariant();
    }
}
