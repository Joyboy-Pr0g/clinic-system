using HomeNursingSystem.Models;

namespace HomeNursingSystem.Data.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Appointment?> GetByIdForPatientAsync(int id, string patientId, CancellationToken ct = default);
    Task<Appointment?> GetByIdForNurseAsync(int id, int nurseProfileId, CancellationToken ct = default);
    Task<Appointment?> GetByIdForClinicAsync(int id, int clinicId, CancellationToken ct = default);
    IQueryable<Appointment> Query();
    Task AddAsync(Appointment entity, CancellationToken ct = default);
    void Update(Appointment entity);
    void Remove(Appointment entity);
    Task SaveChangesAsync(CancellationToken ct = default);
}
