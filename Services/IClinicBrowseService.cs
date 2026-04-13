using HomeNursingSystem.ViewModels;

namespace HomeNursingSystem.Services;

public interface IClinicBrowseService
{
    Task<IReadOnlyList<ClinicListItemVM>> ListVerifiedAsync(string? search, string? city, string? specialty, CancellationToken ct = default);

    Task<IReadOnlyList<string>> DistinctClinicCitiesAsync(CancellationToken ct = default);
    Task<ClinicDetailsVM?> GetDetailsAsync(int id, CancellationToken ct = default);
}
