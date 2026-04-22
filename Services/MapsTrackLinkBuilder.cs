using System.Globalization;
using HomeNursingSystem.Models;

namespace HomeNursingSystem.Services;

/// <summary>روابط Google Maps (فتح التطبيق/المتصفح) بدون خريطة مدمجة في الموقع.</summary>
public static class MapsTrackLinkBuilder
{
    private const double FreshLocationMinutes = 45;

    public static (string Url, string Label)? TryForPatientTrackingProvider(Appointment appt)
    {
        // أزل الشرط للسماح بالتتبع في جميع الحالات
        // if (appt.Status == AppointmentStatuses.Cancelled || appt.Status == AppointmentStatuses.Completed)
        //     return null;

        double? lat = null;
        double? lng = null;
        var label = "فتح في Google Maps";

        if (appt.NurseProfile != null)
        {
            var n = appt.NurseProfile;
            // استخدم آخر موقع معروف للممرض، حتى لو لم يكن طازجاً جداً
            if (n.LastLatitude.HasValue && n.LastLongitude.HasValue)
            {
                lat = n.LastLatitude;
                lng = n.LastLongitude;
                label = "تتبع آخر موقع للممرض/ة في Google Maps";
            }
            else if (appt.Latitude.HasValue && appt.Longitude.HasValue)
            {
                lat = appt.Latitude;
                lng = appt.Longitude;
                label = "الاتجاهات إلى موقع الزيارة";
            }
        }
        else if (appt.Clinic != null)
        {
            var cl = appt.Clinic;
            var clOk = Math.Abs(cl.Latitude) > 0.0001 || Math.Abs(cl.Longitude) > 0.0001;
            if (clOk)
            {
                lat = cl.Latitude;
                lng = cl.Longitude;
                label = "الاتجاهات إلى العيادة في Google Maps";
            }
            else if (appt.Latitude.HasValue && appt.Longitude.HasValue)
            {
                lat = appt.Latitude;
                lng = appt.Longitude;
                label = "الاتجاهات إلى موقع الزيارة";
            }
        }

        if (!lat.HasValue || !lng.HasValue)
            return null;

        var url = $"https://www.google.com/maps/search/?api=1&query={lat.Value.ToString(CultureInfo.InvariantCulture)},{lng.Value.ToString(CultureInfo.InvariantCulture)}";
        return (url, label);
    }

    /// <summary>الممرض/العيادة: الاتجاه إلى المريض (موقع حي حديث أو عنوان الزيارة).</summary>
    public static (string Url, string Label)? TryForProviderTrackingPatient(Appointment appt)
    {
        if (appt.Status == AppointmentStatuses.Cancelled || appt.Status == AppointmentStatuses.Completed)
            return null;

        double? lat = null;
        double? lng = null;
        var p = appt.Patient;
        if (p != null && IsFresh(p.LastLiveLocationAt) && p.Latitude.HasValue && p.Longitude.HasValue)
        {
            lat = p.Latitude;
            lng = p.Longitude;
        }
        else if (appt.Latitude.HasValue && appt.Longitude.HasValue)
        {
            lat = appt.Latitude;
            lng = appt.Longitude;
        }

        if (!lat.HasValue || !lng.HasValue)
            return null;

        var url = $"https://www.google.com/maps/dir/?api=1&destination={lat.Value.ToString(CultureInfo.InvariantCulture)},{lng.Value.ToString(CultureInfo.InvariantCulture)}";
        return (url, "الاتجاهات إلى المريض / موقع الزيارة في Google Maps");
    }

    private static bool IsFresh(DateTime? updatedAtUtc)
    {
        if (!updatedAtUtc.HasValue) return false;
        return (DateTime.UtcNow - updatedAtUtc.Value).TotalMinutes <= FreshLocationMinutes;
    }
}
