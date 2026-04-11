using HomeNursingSystem.Data.Repositories;
using HomeNursingSystem.Models;
using HomeNursingSystem.Services;
using HomeNursingSystem.ViewModels;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Controllers;

[Authorize(Roles = AppRoles.Patient)]
[Route("patient")]
public class PatientController : Controller
{
    private readonly IAppointmentService _appointments;
    private readonly IAppointmentRepository _apptRepo;
    private readonly IRatingService _ratings;
    private readonly UserManager<ApplicationUser> _users;
    private readonly INurseService _nurses;
    private readonly IClinicBrowseService _clinics;
    private readonly HomeNursingSystem.Data.ApplicationDbContext _db;
    private readonly IFileUploadService _files;
    private readonly IConfiguration _config;

    public PatientController(
        IAppointmentService appointments,
        IAppointmentRepository apptRepo,
        IRatingService ratings,
        UserManager<ApplicationUser> users,
        INurseService nurses,
        IClinicBrowseService clinics,
        HomeNursingSystem.Data.ApplicationDbContext db,
        IFileUploadService files,
        IConfiguration config)
    {
        _appointments = appointments;
        _apptRepo = apptRepo;
        _ratings = ratings;
        _users = users;
        _nurses = nurses;
        _clinics = clinics;
        _db = db;
        _files = files;
        _config = config;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var vm = await _appointments.GetPatientDashboardAsync(user!.Id, ct);
        return View(vm);
    }

    [HttpGet("book")]
    public async Task<IActionResult> Book(int? nurseId, int? clinicId, CancellationToken ct)
    {
        var vm = new AppointmentBookVM
        {
            GoogleMapsApiKey = _config["GoogleMapsApiKey"],
            NurseProfileId = nurseId,
            ClinicId = clinicId,
            BookType = nurseId.HasValue ? "Nurse" : "Clinic"
        };
        if (clinicId.HasValue) vm.BookType = "Clinic";
        await PopulateBookPageAsync(vm, ct);
        return View(vm);
    }

    private async Task PopulateBookPageAsync(AppointmentBookVM vm, CancellationToken ct)
    {
        ViewBag.Nurses = await _nurses.BrowseAsync(null, null, null, true, null, ct);
        ViewBag.Clinics = await _clinics.ListVerifiedAsync(null, ct);
        ViewBag.Services = await _db.Services.AsNoTracking().Where(s => s.IsActive).ToListAsync(ct);

        var locked = (vm.NurseProfileId ?? 0) > 0 || (vm.ClinicId ?? 0) > 0;
        ViewBag.ProviderLocked = locked;
        ViewBag.ProviderDisplayName = (string?)null;
        if (vm.NurseProfileId is int npid && npid > 0)
        {
            ViewBag.ProviderDisplayName = await _db.NurseProfiles.AsNoTracking()
                .Where(n => n.NurseProfileId == npid)
                .Select(n => n.User.FullName)
                .FirstOrDefaultAsync(ct);
        }
        else if (vm.ClinicId is int cid && cid > 0)
        {
            ViewBag.ProviderDisplayName = await _db.Clinics.AsNoTracking()
                .Where(c => c.ClinicId == cid)
                .Select(c => c.ClinicName)
                .FirstOrDefaultAsync(ct);
        }
    }

    /// <summary>إعادة توجيه قديمة من واجهات "احجز الآن" إلى الحجز الداخلي.</summary>
    [HttpGet("chat/start")]
    public async Task<IActionResult> StartChat(int? nurseId, int? clinicId, CancellationToken ct)
    {
        if (nurseId.HasValue)
        {
            var exists = await _db.NurseProfiles.AsNoTracking()
                .AnyAsync(n => n.NurseProfileId == nurseId.Value && n.IsVerified && n.IsAvailable, ct);
            if (!exists)
            {
                TempData["Error"] = "الممرض غير متاح للحجز حاليًا.";
                return RedirectToAction("Browse", "Nurse");
            }

            return RedirectToAction(nameof(Book), new { nurseId = nurseId.Value });
        }

        if (clinicId.HasValue)
        {
            var exists = await _db.Clinics.AsNoTracking()
                .AnyAsync(c => c.ClinicId == clinicId.Value && c.IsVerified && c.IsActive, ct);
            if (!exists)
            {
                TempData["Error"] = "العيادة غير متاحة للحجز حاليًا.";
                return RedirectToAction("Index", "Clinic");
            }

            return RedirectToAction(nameof(Book), new { clinicId = clinicId.Value });
        }

        TempData["Error"] = "لم يتم تحديد جهة الحجز.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost("book")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(AppointmentBookVM model, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        if (!ModelState.IsValid)
        {
            model.GoogleMapsApiKey = _config["GoogleMapsApiKey"];
            await PopulateBookPageAsync(model, ct);
            return View(model);
        }

        var (ok, err, id) = await _appointments.BookAsync(model, user!.Id, ct);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, err!);
            model.GoogleMapsApiKey = _config["GoogleMapsApiKey"];
            await PopulateBookPageAsync(model, ct);
            return View(model);
        }

        string? providerName = null;
        if (string.Equals(model.BookType, "Nurse", StringComparison.OrdinalIgnoreCase) && model.NurseProfileId is int npid && npid > 0)
        {
            providerName = await _db.NurseProfiles.AsNoTracking()
                .Where(n => n.NurseProfileId == npid)
                .Select(n => n.User.FullName)
                .FirstOrDefaultAsync(ct);
        }
        else if (string.Equals(model.BookType, "Clinic", StringComparison.OrdinalIgnoreCase) && model.ClinicId is int cid && cid > 0)
        {
            providerName = await _db.Clinics.AsNoTracking()
                .Where(c => c.ClinicId == cid)
                .Select(c => c.ClinicName)
                .FirstOrDefaultAsync(ct);
        }

        TempData["Success"] = $"تم إرسال طلب الحجز لدى {providerName ?? "مقدم الخدمة"}. الطلب بانتظار الموافقة — يمكنك متابعة التفاصيل أدناه.";
        return RedirectToAction(nameof(AppointmentDetail), new { id = id!.Value });
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> Appointments(string? status, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var q = _apptRepo.Query().Where(a => a.PatientId == user!.Id);
        if (!string.IsNullOrEmpty(status))
            q = q.Where(a => a.Status == status);
        var list = await q.Include(a => a.Service).Include(a => a.Rating).OrderByDescending(a => a.AppointmentDate).ToListAsync(ct);
        ViewBag.Status = status;
        return View(list);
    }

    [HttpGet("appointments/{id:int}")]
    public async Task<IActionResult> AppointmentDetail(int id, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var appt = await _apptRepo.GetByIdForPatientAsync(id, user!.Id, ct);
        if (appt == null) return NotFound();
        ViewBag.CurrentUserId = user!.Id;
        ViewBag.CanRate = appt.Status == AppointmentStatuses.Completed && appt.Rating == null;
        var maps = MapsTrackLinkBuilder.TryForPatientTrackingProvider(appt);
        ViewBag.MapsDirectUrl = maps?.Url;
        ViewBag.MapsDirectLabel = maps?.Label;

        return View(appt);
    }

    /// <summary>أحدث رابط Google Maps (إحداثيات من قاعدة البيانات عند كل طلب).</summary>
    [HttpGet("appointments/{id:int}/maps-link")]
    public async Task<IActionResult> AppointmentMapsLink(int id, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var appt = await _apptRepo.GetByIdForPatientAsync(id, user!.Id, ct);
        if (appt == null) return NotFound();
        var link = MapsTrackLinkBuilder.TryForPatientTrackingProvider(appt);
        if (link == null)
            return Json(new { error = "لا تتوفر إحداثيات لفتح الخرائط لهذا الموعد حالياً." });
        return Json(new { url = link.Value.Url, label = link.Value.Label });
    }

    [HttpPost("appointments/{id:int}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var appt = await _apptRepo.GetByIdForPatientAsync(id, user!.Id, ct);
        if (appt == null) return NotFound();
        appt.Status = AppointmentStatuses.Cancelled;
        appt.UpdatedAt = DateTime.UtcNow;
        _apptRepo.Update(appt);
        await _apptRepo.SaveChangesAsync(ct);
        TempData["Success"] = "تم إلغاء الموعد.";
        return RedirectToAction(nameof(Appointments));
    }

    [HttpPost("rate/{appointmentId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int appointmentId, RatingVM model, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var (ok, err) = await _ratings.SubmitRatingAsync(user!.Id, appointmentId, model.Stars, model.Comment, ct);
        if (!ok)
            TempData["Error"] = err;
        else
            TempData["Success"] = "شكراً لتقييمك.";
        return RedirectToAction(nameof(AppointmentDetail), new { id = appointmentId });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var u = await _users.FindByIdAsync(user!.Id);
        var vm = new PatientProfileVM
        {
            FullName = u!.FullName,
            PhoneNumber = u.PhoneNumber,
            City = u.City,
            Neighborhood = u.Neighborhood,
            Street = u.Street,
            ProfileImagePath = u.ProfileImagePath
        };
        return View(vm);
    }

    [HttpPost("profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(PatientProfileVM model, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        var u = await _users.FindByIdAsync(user!.Id);
        if (!ModelState.IsValid)
            return View(model);
        u!.FullName = model.FullName;
        u.PhoneNumber = model.PhoneNumber;
        u.City = model.City;
        u.Neighborhood = model.Neighborhood;
        u.Street = model.Street;
        if (model.ProfileImage != null)
        {
            var path = await _files.SaveImageAsync(model.ProfileImage, "profiles", ct);
            if (path != null)
                u.ProfileImagePath = path;
        }
        await _users.UpdateAsync(u);
        TempData["Success"] = "تم حفظ الملف الشخصي.";
        return RedirectToAction(nameof(Profile));
    }
}
