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

    public async Task<IReadOnlyList<ClinicListItemVM>> ListVerifiedAsync(string? search, string? city, string? specialty, CancellationToken ct = default)
    {
        var q = _clinics.QueryVerified();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(c => c.ClinicName.Contains(s)
                || (c.City != null && c.City.Contains(s))
                || c.Address.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var cty = city.Trim();
            q = q.Where(c => c.City != null && c.City.Contains(cty));
        }

        if (!string.IsNullOrWhiteSpace(specialty))
        {
            var sp = specialty.Trim();
            q = q.Where(c =>
                (c.Description != null && c.Description.Contains(sp))
                || c.ClinicName.Contains(sp)
                || (c.Neighborhood != null && c.Neighborhood.Contains(sp)));
        }

        return await q
            .OrderByDescending(c => c.AverageRating)
            .Select(c => new ClinicListItemVM
            {
                ClinicId = c.ClinicId,
                ClinicName = c.ClinicName,
                LogoImagePath = c.LogoImagePath,
                CoverImagePath = c.CoverImagePath,
                Address = c.Address,
                Neighborhood = c.Neighborhood,
                City = c.City,
                OpeningHours = c.OpeningHours,
                AverageRating = c.AverageRating,
                Latitude = c.Latitude,
                Longitude = c.Longitude
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> DistinctClinicCitiesAsync(CancellationToken ct = default)
    {
        return await _clinics.QueryVerified()
            .Where(c => c.City != null && c.City != "")
            .Select(c => c.City!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);
    }

    public async Task<ClinicDetailsVM?> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        var c = await _clinics.GetByIdAsync(id, ct);
        if (c == null || !c.IsVerified || !c.IsActive)
            return null;

        var clinicServices = await _db.ClinicServices.AsNoTracking()
            .Where(s => s.ClinicId == c.ClinicId)
            .OrderBy(s => s.Name)
            .Select(s => new ClinicServicePublicVM
            {
                Name = s.Name,
                Price = s.Price
            })
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
            ClinicServices = clinicServices,
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
