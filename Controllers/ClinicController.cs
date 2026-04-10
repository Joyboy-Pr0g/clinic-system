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

[Authorize(Roles = AppRoles.ClinicOwner)]
[Route("clinic")]
public class ClinicController : Controller
{
    private readonly IClinicBrowseService _browse;
    private readonly IClinicRepository _clinics;
    private readonly IAppointmentService _appointments;
    private readonly IAppointmentRepository _apptRepo;
    private readonly UserManager<ApplicationUser> _users;
    private readonly ApplicationDbContext _db;
    private readonly IFileUploadService _files;
    private readonly IConfiguration _config;

    public ClinicController(
        IClinicBrowseService browse,
        IClinicRepository clinics,
        IAppointmentService appointments,
        IAppointmentRepository apptRepo,
        UserManager<ApplicationUser> users,
        ApplicationDbContext db,
        IFileUploadService files,
        IConfiguration config)
    {
        _browse = browse;
        _clinics = clinics;
        _appointments = appointments;
        _apptRepo = apptRepo;
        _users = users;
        _db = db;
        _files = files;
        _config = config;
    }

    [AllowAnonymous]
    [HttpGet("/clinics")]
    public async Task<IActionResult> Index(string? search, CancellationToken ct)
    {
        ViewBag.Search = search;
        var list = await _browse.ListVerifiedAsync(search, ct);
        return View("PublicIndex", list);
    }

    [AllowAnonymous]
    [HttpGet("/clinics/{id:int}")]
    public async Task<IActionResult> PublicDetails(int id, CancellationToken ct)
    {
        var vm = await _browse.GetDetailsAsync(id, ct);
        if (vm == null) return NotFound();
        ViewBag.GoogleMapsApiKey = _config["GoogleMapsApiKey"];
        return View("PublicDetails", vm);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var clinic = await _clinics.GetByOwnerIdAsync(user!.Id, ct);
        if (clinic == null) return RedirectToAction("PendingApproval", "Account");
        var vm = await _appointments.GetClinicDashboardAsync(clinic.ClinicId, ct);
        return View(vm);
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> Appointments(string? status, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var clinic = await _clinics.GetByOwnerIdAsync(user!.Id, ct);
        if (clinic == null) return NotFound();
        var q = _apptRepo.Query().Where(a => a.ClinicId == clinic.ClinicId);
        if (!string.IsNullOrEmpty(status))
            q = q.Where(a => a.Status == status);
        var list = await q.Include(a => a.Patient).Include(a => a.Service).OrderByDescending(a => a.AppointmentDate).ToListAsync(ct);
        ViewBag.Status = status;
        return View(list);
    }

    [HttpGet("appointments/{id:int}")]
    public async Task<IActionResult> AppointmentDetail(int id, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var clinic = await _clinics.GetByOwnerIdAsync(user!.Id, ct);
        if (clinic == null) return NotFound();
        var appt = await _apptRepo.GetByIdForClinicAsync(id, clinic.ClinicId, ct);
        if (appt == null) return NotFound();
        ViewBag.GoogleMapsApiKey = _config["GoogleMapsApiKey"];
        return View(appt);
    }

    [HttpPost("appointments/{id:int}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, string status, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var (ok, err) = await _appointments.UpdateStatusAsync(id, status, user!.Id, AppRoles.ClinicOwner, ct);
        if (!ok) TempData["Error"] = err;
        else TempData["Success"] = "تم التحديث.";
        return RedirectToAction(nameof(AppointmentDetail), new { id });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var c = await _clinics.GetByOwnerIdAsync(user!.Id, ct);
        if (c == null) return NotFound();
        var vm = new ClinicProfileEditVM
        {
            ClinicId = c.ClinicId,
            ClinicName = c.ClinicName,
            Description = c.Description,
            Address = c.Address,
            Neighborhood = c.Neighborhood,
            City = c.City,
            Latitude = c.Latitude,
            Longitude = c.Longitude,
            OpeningHours = c.OpeningHours,
            PhoneNumber = c.PhoneNumber,
            Email = c.Email,
            LogoImagePath = c.LogoImagePath,
            CoverImagePath = c.CoverImagePath,
            GoogleMapsApiKey = _config["GoogleMapsApiKey"]
        };
        return View(vm);
    }

    [HttpPost("profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ClinicProfileEditVM model, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var c = await _db.Clinics.FirstAsync(x => x.OwnerId == user!.Id, ct);
        if (!ModelState.IsValid)
        {
            model.GoogleMapsApiKey = _config["GoogleMapsApiKey"];
            return View(model);
        }
        c.ClinicName = model.ClinicName;
        c.Description = model.Description;
        c.Address = model.Address;
        c.Neighborhood = model.Neighborhood;
        c.City = model.City;
        c.Latitude = model.Latitude;
        c.Longitude = model.Longitude;
        c.OpeningHours = model.OpeningHours;
        c.PhoneNumber = model.PhoneNumber;
        c.Email = model.Email;
        if (model.LogoFile != null)
        {
            var p = await _files.SaveImageAsync(model.LogoFile, "clinics", ct);
            if (p != null) c.LogoImagePath = p;
        }
        if (model.CoverFile != null)
        {
            var p = await _files.SaveImageAsync(model.CoverFile, "clinics", ct);
            if (p != null) c.CoverImagePath = p;
        }
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تم حفظ بيانات العيادة.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet("reviews")]
    public async Task<IActionResult> Reviews(CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var clinic = await _clinics.GetByOwnerIdAsync(user!.Id, ct);
        if (clinic == null) return NotFound();
        var ratings = await _db.Ratings.AsNoTracking()
            .Include(r => r.Patient)
            .Where(r => r.ClinicId == clinic.ClinicId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
        return View(ratings);
    }
}
