using System.ComponentModel.DataAnnotations.Schema;

namespace HomeNursingSystem.Models;

[Table("NurseServices")]
public class NurseServiceLink
{
    public int NurseServiceId { get; set; }
    public int NurseProfileId { get; set; }
    public NurseProfile NurseProfile { get; set; } = null!;
    public int ServiceId { get; set; }
    public MedicalService Service { get; set; } = null!;
    public decimal CustomPrice { get; set; }
}
