using HomeNursingSystem.Models;

namespace HomeNursingSystem.Data.Repositories;

public interface INurseProfileRepository
{
    Task<NurseProfile?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<NurseProfile?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    IQueryable<NurseProfile> QueryBrowse();
}
