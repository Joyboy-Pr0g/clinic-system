using HomeNursingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Data.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _db;

    public AppointmentRepository(ApplicationDbContext db) => _db = db;

    public IQueryable<Appointment> Query() =>
        _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.NurseProfile)!.ThenInclude(n => n!.User)
            .Include(a => a.Clinic)
            .Include(a => a.Service)
            .Include(a => a.ClinicService)
            .Include(a => a.NurseListingService)
            .Include(a => a.Rating);

    public Task<Appointment?> GetByIdAsync(int id, CancellationToken ct = default) =>
        Query().FirstOrDefaultAsync(a => a.AppointmentId == id, ct);

    public Task<Appointment?> GetByIdForPatientAsync(int id, string patientId, CancellationToken ct = default) =>
        Query().FirstOrDefaultAsync(a => a.AppointmentId == id && a.PatientId == patientId, ct);

    public Task<Appointment?> GetByIdForNurseAsync(int id, int nurseProfileId, CancellationToken ct = default) =>
        Query().FirstOrDefaultAsync(a => a.AppointmentId == id && a.NurseProfileId == nurseProfileId, ct);

    public Task<Appointment?> GetByIdForClinicAsync(int id, int clinicId, CancellationToken ct = default) =>
        Query().FirstOrDefaultAsync(a => a.AppointmentId == id && a.ClinicId == clinicId, ct);

    public async Task AddAsync(Appointment entity, CancellationToken ct = default) =>
        await _db.Appointments.AddAsync(entity, ct);

    public void Update(Appointment entity) => _db.Appointments.Update(entity);

    public void Remove(Appointment entity) => _db.Appointments.Remove(entity);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
