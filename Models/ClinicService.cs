using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.Models;

/// <summary>خدمة معروضة من العيادة (اسم وسعر خاص بالعيادة).</summary>
public class ClinicService
{
    public int ClinicServiceId { get; set; }

    public int ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;

    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
