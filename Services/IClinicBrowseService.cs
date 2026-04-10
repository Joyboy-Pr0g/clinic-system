using HomeNursingSystem.ViewModels;

namespace HomeNursingSystem.Services;

public interface IClinicBrowseService
{
    Task<IReadOnlyList<ClinicListItemVM>> ListVerifiedAsync(string? search, CancellationToken ct = default);
    Task<ClinicDetailsVM?> GetDetailsAsync(int id, CancellationToken ct = default);
}
