using HomeNursingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Data.Repositories;

public class ClinicRepository : IClinicRepository
{
    private readonly ApplicationDbContext _db;

    public ClinicRepository(ApplicationDbContext db) => _db = db;

    public IQueryable<Clinic> QueryVerified() =>
        _db.Clinics.AsNoTracking().Where(c => c.IsVerified && c.IsActive);

    public Task<Clinic?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Clinics
            .Include(c => c.Owner)
            .Include(c => c.Ratings).ThenInclude(r => r.Patient)
            .FirstOrDefaultAsync(c => c.ClinicId == id, ct);

    public Task<Clinic?> GetByOwnerIdAsync(string ownerId, CancellationToken ct = default) =>
        _db.Clinics.FirstOrDefaultAsync(c => c.OwnerId == ownerId, ct);
}
