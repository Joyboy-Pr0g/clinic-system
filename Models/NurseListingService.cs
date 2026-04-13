using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.Models;

/// <summary>خدمة يعرضها الممرض باسم وسعر حرّين (مثل خدمات العيادة).</summary>
public class NurseListingService
{
    public int NurseListingServiceId { get; set; }

    public int NurseProfileId { get; set; }
    public NurseProfile NurseProfile { get; set; } = null!;

    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
