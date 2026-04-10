using HomeNursingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Data.Repositories;

public class NurseProfileRepository : INurseProfileRepository
{
    private readonly ApplicationDbContext _db;

    public NurseProfileRepository(ApplicationDbContext db) => _db = db;

    public IQueryable<NurseProfile> QueryBrowse() =>
        _db.NurseProfiles
            .AsNoTracking()
            .Include(n => n.User)
            .Include(n => n.NurseServices).ThenInclude(ns => ns.Service)
            .Where(n => n.IsVerified && n.User.IsActive);

    public Task<NurseProfile?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default) =>
        _db.NurseProfiles
            .Include(n => n.User)
            .Include(n => n.NurseServices).ThenInclude(ns => ns.Service)
            .Include(n => n.Ratings).ThenInclude(r => r.Patient)
            .FirstOrDefaultAsync(n => n.NurseProfileId == id, ct);

    public Task<NurseProfile?> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        _db.NurseProfiles
            .Include(n => n.NurseServices).ThenInclude(ns => ns.Service)
            .FirstOrDefaultAsync(n => n.UserId == userId, ct);
}
