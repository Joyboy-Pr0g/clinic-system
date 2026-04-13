using HomeNursingSystem.Data.Repositories;
using HomeNursingSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Services;

public class NurseService : INurseService
{
    private readonly INurseProfileRepository _nurses;

    public NurseService(INurseProfileRepository nurses) => _nurses = nurses;

    public async Task<IReadOnlyList<NurseListItemVM>> BrowseAsync(
        string? neighborhood,
        int? serviceId,
        decimal? minRating,
        bool? availableOnly,
        string? search,
        CancellationToken ct = default)
    {
        var q = _nurses.QueryBrowse();

        if (!string.IsNullOrWhiteSpace(neighborhood))
            q = q.Where(n => n.User.Neighborhood != null && n.User.Neighborhood.Contains(neighborhood));
        if (serviceId.HasValue)
            q = q.Where(n => n.NurseServices.Any(ns => ns.ServiceId == serviceId.Value));
        if (minRating.HasValue)
            q = q.Where(n => n.AverageRating >= minRating.Value);
        if (availableOnly == true)
            q = q.Where(n => n.IsAvailable);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(n => n.User.FullName.Contains(s) || n.Specialization.Contains(s));
        }

        var list = await q
            .OrderByDescending(n => n.AverageRating)
            .ToListAsync(ct);
        return list.ConvertAll(n => new NurseListItemVM
        {
            NurseProfileId = n.NurseProfileId,
            FullName = n.User.FullName,
            ProfileImage = n.User.ProfileImagePath,
            Specialization = n.Specialization,
            AverageRating = n.AverageRating,
            TotalReviews = n.TotalReviews,
            IsAvailable = n.IsAvailable,
            Neighborhood = n.User.Neighborhood,
            ServiceNames = n.NurseServices.Select(ns => ns.Service.ServiceName).ToList()
                .ConvertAll(ServiceNameLocalizer.Localize)
        });
    }

    public async Task<NurseDetailsVM?> GetPublicDetailsAsync(int id, CancellationToken ct = default)
    {
        var n = await _nurses.GetByIdWithDetailsAsync(id, ct);
        if (n == null || !n.IsVerified || !n.User.IsActive)
            return null;

        return new NurseDetailsVM
        {
            NurseProfileId = n.NurseProfileId,
            FullName = n.User.FullName,
            ProfileImage = n.User.ProfileImagePath,
            Specialization = n.Specialization,
            YearsOfExperience = n.YearsOfExperience,
            Bio = n.Bio,
            IsVerified = n.IsVerified,
            AverageRating = n.AverageRating,
            TotalReviews = n.TotalReviews,
            IsAvailable = n.IsAvailable,
            Services = n.NurseServices.Select(ns => new NurseServicePriceVM
            {
                ServiceId = ns.ServiceId,
                ServiceName = ServiceNameLocalizer.Localize(ns.Service.ServiceName),
                Price = ns.CustomPrice
            }).ToList(),
            Reviews = n.Ratings
                .Where(r => r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .Take(20)
                .Select(r => new RatingDisplayVM
                {
                    Stars = r.Stars,
                    Comment = r.Comment,
                    PatientName = r.Patient.FullName,
                    CreatedAt = r.CreatedAt
                })
                .ToList()
        };
    }
}
