using HomeNursingSystem.Models;

namespace HomeNursingSystem.Data.Repositories;

public interface IClinicRepository
{
    Task<Clinic?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Clinic?> GetByOwnerIdAsync(string ownerId, CancellationToken ct = default);
    IQueryable<Clinic> QueryVerified();
}
