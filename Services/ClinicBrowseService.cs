using HomeNursingSystem.Data;
using HomeNursingSystem.Data.Repositories;
using HomeNursingSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Services;

public class ClinicBrowseService : IClinicBrowseService
{
    private readonly IClinicRepository _clinics;
    private readonly ApplicationDbContext _db;

    public ClinicBrowseService(IClinicRepository clinics, ApplicationDbContext db)
    {
        _clinics = clinics;
        _db = db;
    }

    public async Task<IReadOnlyList<ClinicListItemVM>> ListVerifiedAsync(string? search, CancellationToken ct = default)
    {
        var q = _clinics.QueryVerified();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(c => c.ClinicName.Contains(s) || (c.City != null && c.City.Contains(s)));
        }

        return await q
            .OrderByDescending(c => c.AverageRating)
            .Select(c => new ClinicListItemVM
            {
                ClinicId = c.ClinicId,
                ClinicName = c.ClinicName,
                LogoImagePath = c.LogoImagePath,
                Address = c.Address,
                Neighborhood = c.Neighborhood,
                City = c.City,
                AverageRating = c.AverageRating,
                Latitude = c.Latitude,
                Longitude = c.Longitude
            })
            .ToListAsync(ct);
    }

    public async Task<ClinicDetailsVM?> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        var c = await _clinics.GetByIdAsync(id, ct);
        if (c == null || !c.IsVerified || !c.IsActive)
            return null;

        var services = await _db.Services.AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.ServiceName)
            .Select(s => new ServiceListItemVM { ServiceId = s.ServiceId, ServiceName = s.ServiceName, BasePrice = s.BasePrice })
            .ToListAsync(ct);

        return new ClinicDetailsVM
        {
            ClinicId = c.ClinicId,
            ClinicName = c.ClinicName,
            Description = c.Description,
            Address = c.Address,
            OpeningHours = c.OpeningHours,
            PhoneNumber = c.PhoneNumber,
            Email = c.Email,
            LogoImagePath = c.LogoImagePath,
            CoverImagePath = c.CoverImagePath,
            AverageRating = c.AverageRating,
            Latitude = c.Latitude,
            Longitude = c.Longitude,
            ServicesOffered = services,
            Reviews = c.Ratings
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
