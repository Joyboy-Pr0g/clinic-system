using HomeNursingSystem.ViewModels;

namespace HomeNursingSystem.Services;

public interface INurseService
{
    Task<IReadOnlyList<NurseListItemVM>> BrowseAsync(string? neighborhood, int? serviceId, decimal? minRating, bool? availableOnly, string? search, CancellationToken ct = default);
    Task<NurseDetailsVM?> GetPublicDetailsAsync(int id, CancellationToken ct = default);
}
