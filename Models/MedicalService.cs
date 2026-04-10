namespace HomeNursingSystem.Models;

public class MedicalService
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconClass { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<NurseServiceLink> NurseServices { get; set; } = new List<NurseServiceLink>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
