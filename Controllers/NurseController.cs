using HomeNursingSystem.Data;
using HomeNursingSystem.Data.Repositories;
using HomeNursingSystem.Models;
using HomeNursingSystem.Services;
using HomeNursingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Controllers;

[Authorize(Roles = AppRoles.Nurse)]
[Route("nurse")]
public class NurseController : Controller
{
    private readonly INurseService _nurseDirectory;
    private readonly INurseProfileRepository _profiles;
    private readonly IAppointmentService _appointments;
    private readonly IAppointmentRepository _apptRepo;
    private readonly UserManager<ApplicationUser> _users;
    private readonly ApplicationDbContext _db;
    private readonly IFileUploadService _files;
    private readonly IConfiguration _config;

    public NurseController(
        INurseService nurseDirectory,
        INurseProfileRepository profiles,
        IAppointmentService appointments,
        IAppointmentRepository apptRepo,
        UserManager<ApplicationUser> users,
        ApplicationDbContext db,
        IFileUploadService files,
        IConfiguration config)
    {
        _nurseDirectory = nurseDirectory;
        _profiles = profiles;
        _appointments = appointments;
        _apptRepo = apptRepo;
        _users = users;
        _db = db;
        _files = files;
        _config = config;
    }

    [AllowAnonymous]
    [HttpGet("/nurses")]
    public async Task<IActionResult> Browse(string? neighborhood, int? serviceId, decimal? minRating, bool? available, string? search, CancellationToken ct)
    {
        ViewBag.Neighborhood = neighborhood;
        ViewBag.ServiceId = serviceId;
        ViewBag.MinRating = minRating;
        ViewBag.Available = available;
        ViewBag.Search = search;
        ViewBag.Services = await _db.Services.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.ServiceName).ToListAsync(ct);
        var list = await _nurseDirectory.BrowseAsync(neighborhood, serviceId, minRating, available, search, ct);
        return View("Browse", list);
    }

    [AllowAnonymous]
    [HttpGet("/nurses/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var vm = await _nurseDirectory.GetPublicDetailsAsync(id, ct);
        if (vm == null)
            return NotFound();
        return View("PublicDetails", vm);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var np = await _profiles.GetByUserIdAsync(user!.Id, ct);
        if (np == null)
            return RedirectToAction("PendingApproval", "Account");
        var vm = await _appointments.GetNurseDashboardAsync(np.NurseProfileId, ct);
        return View(vm);
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> Appointments(string? status, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var np = await _profiles.GetByUserIdAsync(user!.Id, ct);
        if (np == null) return NotFound();
        var q = _apptRepo.Query().Where(a => a.NurseProfileId == np.NurseProfileId);
        if (!string.IsNullOrEmpty(status))
            q = q.Where(a => a.Status == status);
        var list = await q.Include(a => a.Patient).Include(a => a.Service).Include(a => a.ClinicService).Include(a => a.NurseListingService).OrderByDescending(a => a.AppointmentDate).ToListAsync(ct);
        ViewBag.Status = status;
        return View(list);
    }

    [HttpGet("appointments/{id:int}")]
    public async Task<IActionResult> AppointmentDetail(int id, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var np = await _profiles.GetByUserIdAsync(user!.Id, ct);
        if (np == null) return NotFound();
        var appt = await _apptRepo.GetByIdForNurseAsync(id, np.NurseProfileId, ct);
        if (appt == null) return NotFound();
        ViewBag.CurrentUserId = user!.Id;
        var maps = MapsTrackLinkBuilder.TryForProviderTrackingPatient(appt);
        ViewBag.MapsDirectUrl = maps?.Url;
        ViewBag.MapsDirectLabel = maps?.Label;

        return View(appt);
    }

    [HttpGet("appointments/{id:int}/maps-link")]
    public async Task<IActionResult> AppointmentMapsLink(int id, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var np = await _profiles.GetByUserIdAsync(user!.Id, ct);
        if (np == null) return NotFound();
        var appt = await _apptRepo.GetByIdForNurseAsync(id, np.NurseProfileId, ct);
        if (appt == null) return NotFound();
        var link = MapsTrackLinkBuilder.TryForProviderTrackingPatient(appt);
        if (link == null)
            return Json(new { error = "لا تتوفر إحداثيات المريض أو عنوان الزيارة في الخرائط حالياً." });
        return Json(new { url = link.Value.Url, label = link.Value.Label });
    }

    [HttpPost("appointments/{id:int}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, string status, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var (ok, err) = await _appointments.UpdateStatusAsync(id, status, user!.Id, AppRoles.Nurse, ct);
        if (!ok)
            TempData["Error"] = err;
        else
            TempData["Success"] = "تم تحديث الحالة.";
        return RedirectToAction(nameof(AppointmentDetail), new { id });
    }

    [HttpPost("appointments/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAppointment(int id, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var np = await _profiles.GetByUserIdAsync(user!.Id, ct);
        if (np == null) return NotFound();
        var appt = await _apptRepo.GetByIdForNurseAsync(id, np.NurseProfileId, ct);
        if (appt == null) return NotFound();
        var st = appt.Status ?? "";
        if (!string.Equals(st, AppointmentStatuses.Cancelled, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(st, AppointmentStatuses.Completed, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "يمكن حذف المواعيد الملغاة أو المكتملة فقط.";
            return RedirectToAction(nameof(Appointments));
        }

        _apptRepo.Remove(appt);
        await _apptRepo.SaveChangesAsync(ct);
        TempData["Success"] = "تم حذف الموعد من سجلك.";
        return RedirectToAction(nameof(Appointments));
    }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var np = await _profiles.GetByUserIdAsync(user!.Id, ct);
        if (np == null) return NotFound();
        var allServices = await _db.Services.AsNoTracking().Where(s => s.IsActive).ToListAsync(ct);
        var selected = np.NurseServices.ToDictionary(ns => ns.ServiceId, ns => ns.CustomPrice);
        var vm = new NurseProfileEditVM
        {
            Specialization = np.Specialization,
            YearsOfExperience = np.YearsOfExperience,
            Bio = np.Bio,
            IsAvailable = np.IsAvailable,
            ServiceRows = allServices.Select(s => new NurseServiceEditRowVM {
                ServiceId = s.ServiceId,
                ServiceName = s.ServiceName,
                Selected = selected.ContainsKey(s.ServiceId),
                CustomPrice = selected.TryGetValue(s.ServiceId, out var p) ? p : s.BasePrice
            }).ToList()
        };
        return View(vm);
    }

    [HttpPost("profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(NurseProfileEditVM model, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var np = await _db.NurseProfiles.Include(n => n.NurseServices).FirstAsync(n => n.UserId == user!.Id, ct);
        np.Specialization = model.Specialization;
        np.YearsOfExperience = model.YearsOfExperience;
        np.Bio = model.Bio;
        np.IsAvailable = model.IsAvailable;

        var keep = model.ServiceRows.Where(r => r.Selected).ToList();
        _db.NurseServices.RemoveRange(np.NurseServices);
        foreach (var row in keep)
        {
            _db.NurseServices.Add(new NurseServiceLink
            {
                NurseProfileId = np.NurseProfileId,
                ServiceId = row.ServiceId,
                CustomPrice = row.CustomPrice
            });
        }
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تم حفظ الملف.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet("reviews")]
    public async Task<IActionResult> Reviews(CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var np = await _profiles.GetByUserIdAsync(user!.Id, ct);
        if (np == null) return NotFound();
        var ratings = await _db.Ratings.AsNoTracking()
            .Include(r => r.Patient)
            .Where(r => r.NurseProfileId == np.NurseProfileId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
        return View(ratings);
    }
}
