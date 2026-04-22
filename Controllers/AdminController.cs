using System.Text.RegularExpressions;
using HomeNursingSystem.Data;
using HomeNursingSystem.Models;
using HomeNursingSystem.Services;
using HomeNursingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Controllers;

[Authorize(Roles = AppRoles.Admin)]
[Route("admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IAppointmentService _appointments;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IFileUploadService _files;
    private readonly IWebHostEnvironment _env;

    public AdminController(
        ApplicationDbContext db,
        IAppointmentService appointments,
        UserManager<ApplicationUser> users,
        IFileUploadService files,
        IWebHostEnvironment env)
    {
        _db = db;
        _appointments = appointments;
        _users = users;
        _files = files;
        _env = env;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var vm = await _appointments.GetAdminDashboardAsync(ct);
        return View(vm);
    }

    [HttpGet("nurses")]
    public async Task<IActionResult> Nurses(CancellationToken ct)
    {
        var list = await _db.NurseProfiles
            .AsNoTracking()
            .Include(n => n.User)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
        return View(list);
    }

    [HttpPost("nurses/{id:int}/verify")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyNurse(int id, CancellationToken ct)
    {
        await _db.NurseProfiles.Where(n => n.NurseProfileId == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsVerified, true)
                .SetProperty(n => n.IsRejectedByAdmin, false)
                .SetProperty(n => n.AdminRejectionNote, (string?)null), ct);
        TempData["Success"] = "تم قبول الممرض والتحقق من حسابه.";
        return RedirectToAction(nameof(Nurses));
    }

    [HttpPost("nurses/{id:int}/reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectNurse(int id, string? note, CancellationToken ct)
    {
        var np = await _db.NurseProfiles.Include(n => n.User).FirstOrDefaultAsync(n => n.NurseProfileId == id, ct);
        if (np == null) return NotFound();
        np.IsVerified = false;
        np.IsRejectedByAdmin = true;
        np.AdminRejectionNote = string.IsNullOrWhiteSpace(note) ? "لم يتم قبول طلب التحقق بعد مراجعة الوثائق." : note.Trim();
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تم رفض طلب الممرض.";
        return RedirectToAction(nameof(Nurses));
    }

    [HttpPost("nurses/{id:int}/suspend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspendNurse(int id, CancellationToken ct)
    {
        var np = await _db.NurseProfiles.Include(n => n.User).FirstAsync(n => n.NurseProfileId == id, ct);
        np.User.IsActive = false;
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تم تعليق حساب الممرض.";
        return RedirectToAction(nameof(Nurses));
    }

    [HttpPost("nurses/{id:int}/unsuspend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnsuspendNurse(int id, CancellationToken ct)
    {
        var np = await _db.NurseProfiles.Include(n => n.User).FirstAsync(n => n.NurseProfileId == id, ct);
        np.User.IsActive = true;
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تم إلغاء تعليق حساب الممرض.";
        return RedirectToAction(nameof(Nurses));
    }

    [HttpPost("nurses/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNurse(int id, CancellationToken ct)
    {
        var np = await _db.NurseProfiles.AsNoTracking().FirstOrDefaultAsync(n => n.NurseProfileId == id, ct);
        if (np == null) return NotFound();
        await AdminDeleteNurseDataAsync(id, ct);
        await AdminDeleteUserContentAsync(np.UserId, ct);
        var user = await _users.FindByIdAsync(np.UserId);
        if (user != null)
            await _users.DeleteAsync(user);
        TempData["Success"] = "تم حذف الممرض وحسابه وبياناته من النظام.";
        return RedirectToAction(nameof(Nurses));
    }

    [HttpGet("nurses/{id:int}/license")]
    public async Task<IActionResult> ViewLicense(int id, CancellationToken ct)
    {
        var path = await _db.NurseProfiles.AsNoTracking()
            .Where(n => n.NurseProfileId == id)
            .Select(n => n.LicenseImagePath)
            .FirstOrDefaultAsync(ct);
        if (string.IsNullOrEmpty(path)) return NotFound();
        var physical = Path.Combine(_env.WebRootPath, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (!System.IO.File.Exists(physical)) return NotFound();
        return PhysicalFile(physical, MimeForLicensePath(physical));
    }

    [HttpGet("clinics/{id:int}/license")]
    public async Task<IActionResult> ViewClinicLicense(int id, CancellationToken ct)
    {
        var path = await _db.Clinics.AsNoTracking()
            .Where(c => c.ClinicId == id)
            .Select(c => c.LicenseDocumentPath)
            .FirstOrDefaultAsync(ct);
        if (string.IsNullOrEmpty(path)) return NotFound();
        var physical = Path.Combine(_env.WebRootPath, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (!System.IO.File.Exists(physical)) return NotFound();
        return PhysicalFile(physical, MimeForLicensePath(physical));
    }

    private static string MimeForLicensePath(string physical)
    {
        return Path.GetExtension(physical).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            _ => "image/jpeg"
        };
    }

    [HttpGet("clinics")]
    public async Task<IActionResult> Clinics(CancellationToken ct)
    {
        var list = await _db.Clinics.AsNoTracking().Include(c => c.Owner).OrderByDescending(c => c.CreatedAt).ToListAsync(ct);
        return View(list);
    }

    [HttpPost("clinics/{id:int}/verify")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyClinic(int id, CancellationToken ct)
    {
        await _db.Clinics.Where(c => c.ClinicId == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.IsVerified, true)
                .SetProperty(c => c.IsRejectedByAdmin, false)
                .SetProperty(c => c.AdminRejectionNote, (string?)null), ct);
        TempData["Success"] = "تم قبول العيادة والتحقق من حسابها.";
        return RedirectToAction(nameof(Clinics));
    }

    [HttpPost("clinics/{id:int}/reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectClinic(int id, string? note, CancellationToken ct)
    {
        var c = await _db.Clinics.FirstOrDefaultAsync(x => x.ClinicId == id, ct);
        if (c == null) return NotFound();
        c.IsVerified = false;
        c.IsRejectedByAdmin = true;
        c.AdminRejectionNote = string.IsNullOrWhiteSpace(note) ? "لم يتم قبول طلب التحقق بعد مراجعة الوثائق." : note.Trim();
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تم رفض طلب العيادة.";
        return RedirectToAction(nameof(Clinics));
    }

    [HttpPost("clinics/{id:int}/suspend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspendClinic(int id, CancellationToken ct)
    {
        var c = await _db.Clinics.Include(x => x.Owner).FirstAsync(x => x.ClinicId == id, ct);
        c.IsActive = false;
        c.Owner.IsActive = false;
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تم تعليق العيادة.";
        return RedirectToAction(nameof(Clinics));
    }

    [HttpPost("clinics/{id:int}/unsuspend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnsuspendClinic(int id, CancellationToken ct)
    {
        var c = await _db.Clinics.Include(x => x.Owner).FirstAsync(x => x.ClinicId == id, ct);
        c.IsActive = true;
        c.Owner.IsActive = true;
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تم إلغاء تعليق العيادة.";
        return RedirectToAction(nameof(Clinics));
    }

    [HttpPost("clinics/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteClinic(int id, CancellationToken ct)
    {
        var exists = await _db.Clinics.AnyAsync(c => c.ClinicId == id, ct);
        if (!exists) return NotFound();
        await AdminDeleteClinicDataAsync(id, ct);
        TempData["Success"] = "تم حذف العيادة وجميع الحجوزات والخدمات المرتبطة بها.";
        return RedirectToAction(nameof(Clinics));
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users(CancellationToken ct)
    {
        var list = await _db.Users.AsNoTracking().OrderBy(u => u.FullName).ToListAsync(ct);
        var roles = new Dictionary<string, IList<string>>();
        foreach (var u in list)
            roles[u.Id] = await _users.GetRolesAsync(u);
        ViewBag.Roles = roles;
        return View(list);
    }

    [HttpPost("users/{id}/suspend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspendUser(string id, CancellationToken ct)
    {
        var u = await _users.FindByIdAsync(id);
        if (u == null) return NotFound();
        u.IsActive = false;
        await _users.UpdateAsync(u);
        TempData["Success"] = "تم تعليق المستخدم.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost("users/{id}/unsuspend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnsuspendUser(string id, CancellationToken ct)
    {
        var u = await _users.FindByIdAsync(id);
        if (u == null) return NotFound();
        u.IsActive = true;
        await _users.UpdateAsync(u);
        TempData["Success"] = "تم إلغاء تعليق المستخدم.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost("users/{id}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id, CancellationToken ct)
    {
        var currentId = _users.GetUserId(User);
        if (string.Equals(id, currentId, StringComparison.Ordinal))
        {
            TempData["Error"] = "لا يمكنك حذف حسابك الحالي.";
            return RedirectToAction(nameof(Users));
        }

        var user = await _db.Users.Include(u => u.NurseProfile).FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user == null) return NotFound();

        if (await _users.IsInRoleAsync(user, AppRoles.Admin))
        {
            var admins = await _users.GetUsersInRoleAsync(AppRoles.Admin);
            if (admins.Count <= 1)
            {
                TempData["Error"] = "لا يمكن حذف آخر مدير في النظام.";
                return RedirectToAction(nameof(Users));
            }
        }

        var ownedClinicIds = await _db.Clinics.Where(c => c.OwnerId == id).Select(c => c.ClinicId).ToListAsync(ct);
        foreach (var cid in ownedClinicIds)
            await AdminDeleteClinicDataAsync(cid, ct);

        if (user.NurseProfile != null)
            await AdminDeleteNurseDataAsync(user.NurseProfile.NurseProfileId, ct);

        await AdminDeleteUserContentAsync(id, ct);
        await _users.DeleteAsync(user);
        TempData["Success"] = "تم حذف المستخدم وجميع البيانات المرتبطة به.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> Appointments(string? status, CancellationToken ct)
    {
        var baseQ = _db.Appointments.AsNoTracking();
        ViewBag.Status = status;
        ViewBag.CountTotal = await baseQ.CountAsync(ct);
        ViewBag.CountPending = await baseQ.CountAsync(a => a.Status == AppointmentStatuses.Pending, ct);
        ViewBag.CountApproved = await baseQ.CountAsync(a =>
            a.Status == AppointmentStatuses.Approved || a.Status == AppointmentStatuses.Confirmed, ct);
        ViewBag.CountInProgress = await baseQ.CountAsync(a => a.Status == AppointmentStatuses.InProgress, ct);
        ViewBag.CountCompleted = await baseQ.CountAsync(a => a.Status == AppointmentStatuses.Completed, ct);
        ViewBag.CountCancelled = await baseQ.CountAsync(a => a.Status == AppointmentStatuses.Cancelled, ct);

        IQueryable<Appointment> q = _db.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Service)
            .Include(a => a.ClinicService)
            .Include(a => a.NurseListingService)
            .Include(a => a.NurseProfile)!.ThenInclude(n => n!.User)
            .Include(a => a.Clinic);
        if (!string.IsNullOrEmpty(status))
            q = q.Where(a => a.Status == status);
        var list = await q
            .OrderByDescending(a => a.AppointmentDate)
            .Take(500)
            .ToListAsync(ct);
        return View(list);
    }

    [HttpPost("appointments/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAppointment(int id, CancellationToken ct)
    {
        var n = await _db.Appointments.Where(a => a.AppointmentId == id).ExecuteDeleteAsync(ct);
        if (n == 0) return NotFound();
        TempData["Success"] = "تم حذف الموعد.";
        var rs = Request.Form["returnStatus"].FirstOrDefault();
        if (string.IsNullOrEmpty(rs))
            return RedirectToAction(nameof(Appointments));
        return RedirectToAction(nameof(Appointments), new { status = rs });
    }

    [HttpGet("articles")]
    public async Task<IActionResult> Articles(CancellationToken ct)
    {
        var list = await _db.Articles.AsNoTracking().OrderByDescending(a => a.CreatedAt).ToListAsync(ct);
        return View(list);
    }

    [HttpGet("articles/create")]
    public IActionResult CreateArticle() => View("ArticleForm", new ArticleVM { IsPublished = false });

    [HttpGet("articles/edit/{id:int}")]
    public async Task<IActionResult> EditArticle(int id, CancellationToken ct)
    {
        var a = await _db.Articles.FindAsync(new object[] { id }, ct);
        if (a == null) return NotFound();
        var vm = new ArticleVM
        {
            ArticleId = a.ArticleId,
            Title = a.Title,
            Summary = a.Summary,
            Content = a.Content,
            Category = a.Category,
            IsNews = a.IsNews,
            IsPublished = a.IsPublished,
            ThumbnailImagePath = a.ThumbnailImagePath,
            Slug = a.Slug
        };
        return View("ArticleForm", vm);
    }

    [HttpPost("articles/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveArticle(ArticleVM model, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        if (!ModelState.IsValid)
            return View("ArticleForm", model);

        Article entity;
        if (model.ArticleId.HasValue)
        {
            entity = await _db.Articles.FirstAsync(a => a.ArticleId == model.ArticleId.Value, ct);
        }
        else
        {
            entity = new Article { AuthorId = user!.Id, CreatedAt = DateTime.UtcNow };
            _db.Articles.Add(entity);
        }

        entity.Title = model.Title;
        entity.Summary = model.Summary;
        entity.Content = model.Content;
        entity.Category = model.Category;
        entity.IsNews = model.IsNews;
        entity.IsPublished = model.IsPublished;
        if (model.IsPublished && entity.PublishedAt == null)
            entity.PublishedAt = DateTime.UtcNow;
        if (!model.IsPublished)
            entity.PublishedAt = null;

        var baseSlug = Slugify(model.Title);
        if (string.IsNullOrEmpty(baseSlug)) baseSlug = "article";
        entity.Slug = await EnsureUniqueSlugAsync(baseSlug, entity.ArticleId, ct);

        if (model.ThumbnailFile != null)
        {
            var path = await _files.SaveImageAsync(model.ThumbnailFile, "articles", ct);
            if (path != null) entity.ThumbnailImagePath = path;
        }

        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تم حفظ المقال.";
        return RedirectToAction(nameof(Articles));
    }

    [HttpPost("articles/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteArticle(int id, CancellationToken ct)
    {
        await _db.Articles.Where(a => a.ArticleId == id).ExecuteDeleteAsync(ct);
        TempData["Success"] = "تم الحذف.";
        return RedirectToAction(nameof(Articles));
    }

    [HttpGet("contacts")]
    public async Task<IActionResult> Contacts(CancellationToken ct)
    {
        var list = await _db.ContactMessages.OrderByDescending(m => m.CreatedAt).ToListAsync(ct);
        return View(list);
    }

    [HttpPost("contacts/{id:int}/read")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkContactRead(int id, CancellationToken ct)
    {
        await _db.ContactMessages.Where(m => m.ContactMessageId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true), ct);
        return RedirectToAction(nameof(Contacts));
    }

    [HttpPost("contacts/{id:int}/reply")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReplyContact(int id, string replyMessage, CancellationToken ct)
    {
        await _db.ContactMessages.Where(m => m.ContactMessageId == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.ReplyMessage, replyMessage)
                .SetProperty(m => m.RepliedAt, DateTime.UtcNow)
                .SetProperty(m => m.IsRead, true), ct);
        TempData["Success"] = "تم حفظ الرد (يمكن إرساله عبر البريد يدوياً).";
        return RedirectToAction(nameof(Contacts));
    }

    [HttpPost("contacts/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteContact(int id, CancellationToken ct)
    {
        await _db.ContactMessages.Where(m => m.ContactMessageId == id).ExecuteDeleteAsync(ct);
        TempData["Success"] = "تم حذف الرسالة.";
        return RedirectToAction(nameof(Contacts));
    }

    [HttpGet("services")]
    public async Task<IActionResult> Services(CancellationToken ct)
    {
        var list = await _db.Services.OrderBy(s => s.ServiceName).ToListAsync(ct);
        return View(list);
    }

    [HttpPost("services/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateService(string serviceName, string? description, decimal basePrice, string? iconClass, CancellationToken ct)
    {
        _db.Services.Add(new MedicalService
        {
            ServiceName = serviceName,
            Description = description,
            BasePrice = basePrice,
            IconClass = iconClass,
            IsActive = true
        });
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تمت الإضافة.";
        return RedirectToAction(nameof(Services));
    }

    [HttpPost("services/{id:int}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleService(int id, CancellationToken ct)
    {
        var s = await _db.Services.FindAsync(new object[] { id }, ct);
        if (s != null)
        {
            s.IsActive = !s.IsActive;
            await _db.SaveChangesAsync(ct);
        }
        return RedirectToAction(nameof(Services));
    }

    [HttpGet("services/{id:int}/edit")]
    public async Task<IActionResult> EditService(int id, CancellationToken ct)
    {
        var s = await _db.Services.AsNoTracking().FirstOrDefaultAsync(x => x.ServiceId == id, ct);
        if (s == null) return NotFound();
        var vm = new ServiceEditVM
        {
            ServiceId = s.ServiceId,
            ServiceName = s.ServiceName,
            Description = s.Description,
            BasePrice = s.BasePrice,
            IconClass = s.IconClass
        };
        return View(vm);
    }

    [HttpPost("services/{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditService(int id, ServiceEditVM model, CancellationToken ct)
    {
        if (id != model.ServiceId) return BadRequest();
        if (!ModelState.IsValid)
            return View(model);
        var s = await _db.Services.FirstOrDefaultAsync(x => x.ServiceId == id, ct);
        if (s == null) return NotFound();
        s.ServiceName = model.ServiceName.Trim();
        s.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        s.BasePrice = model.BasePrice;
        s.IconClass = string.IsNullOrWhiteSpace(model.IconClass) ? null : model.IconClass.Trim();
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تم تحديث الخدمة.";
        return RedirectToAction(nameof(Services));
    }

    [HttpPost("services/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteService(int id, CancellationToken ct)
    {
        var s = await _db.Services.FirstOrDefaultAsync(x => x.ServiceId == id, ct);
        if (s != null)
        {
            s.IsActive = false;
            await _db.SaveChangesAsync(ct);
            TempData["Success"] = "تم إلغاء تفعيل الخدمة.";
        }
        return RedirectToAction(nameof(Services));
    }

    [HttpPost("services/{id:int}/purge")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PurgeService(int id, CancellationToken ct)
    {
        var used = await _db.Appointments.AnyAsync(a => a.ServiceId == id, ct)
            || await _db.NurseServices.AnyAsync(ns => ns.ServiceId == id, ct);
        if (used)
        {
            TempData["Error"] = "لا يمكن الحذف النهائي: الخدمة مرتبطة بحجوزات أو ممرضين. عطّلها بدلاً من ذلك.";
            return RedirectToAction(nameof(Services));
        }

        await _db.Services.Where(s => s.ServiceId == id).ExecuteDeleteAsync(ct);
        TempData["Success"] = "تم حذف الخدمة نهائياً من النظام.";
        return RedirectToAction(nameof(Services));
    }

    [HttpGet("articles/{id:int}/details")]
    public async Task<IActionResult> ArticleDetails(int id, CancellationToken ct)
    {
        var a = await _db.Articles.AsNoTracking().FirstOrDefaultAsync(x => x.ArticleId == id, ct);
        if (a == null) return NotFound();
        return View(a);
    }

    [HttpGet("ratings")]
    public async Task<IActionResult> Ratings(CancellationToken ct)
    {
        var list = await _db.Ratings.AsNoTracking()
            .Include(r => r.Patient)
            .Include(r => r.Appointment)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
        return View(list);
    }

    [HttpPost("ratings/{id:int}/approve")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveRating(int id, [FromForm] string approved, CancellationToken ct)
    {
        var r = await _db.Ratings.FindAsync(new object[] { id }, ct);
        if (r == null) return NotFound();
        r.IsApproved = string.Equals(approved, "true", StringComparison.OrdinalIgnoreCase);
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تم التحديث.";
        return RedirectToAction(nameof(Ratings));
    }

    [HttpPost("ratings/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRating(int id, CancellationToken ct)
    {
        await _db.Ratings.Where(r => r.RatingId == id).ExecuteDeleteAsync(ct);
        TempData["Success"] = "تم حذف التقييم.";
        return RedirectToAction(nameof(Ratings));
    }

    [HttpGet("reports")]
    public async Task<IActionResult> Reports(CancellationToken ct)
    {
        // Get data for charts
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var yearStart = new DateTime(today.Year, 1, 1);

        // Basic dashboard stats
        var totalUsers = await _db.Users.CountAsync(ct);
        var totalNurses = await _db.NurseProfiles.CountAsync(ct);
        var totalClinics = await _db.Clinics.CountAsync(ct);
        var appointmentsToday = await _db.Appointments.CountAsync(a => a.AppointmentDate >= today && a.AppointmentDate < today.AddDays(1), ct);
        var revenueEstimate = await _db.Appointments
            .Where(a => a.Status == AppointmentStatuses.Completed && a.UpdatedAt >= monthStart)
            .SumAsync(a => (decimal?)a.TotalPrice, ct) ?? 0;
        var pendingVerifications = await _db.NurseProfiles.CountAsync(n => !n.IsVerified, ct)
            + await _db.Clinics.CountAsync(c => !c.IsVerified, ct);

        // Appointments by status
        var statusCounts = await _db.Appointments
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        // Revenue by month (last 12 months)
        var revenueByMonth = _db.Appointments
            .Where(a => a.Status == AppointmentStatuses.Completed && a.CreatedAt >= yearStart.AddMonths(-11))
            .GroupBy(a => new { Year = a.CreatedAt.Year, Month = a.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(a => a.TotalPrice) })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .AsEnumerable()
            .Select(g => new { Month = $"{g.Year}-{g.Month:D2}", Revenue = g.Revenue })
            .ToList();

        // Users by role
        var usersByRole = new List<dynamic>();
        var patients = await _db.Users.CountAsync(u => u.Role == AppRoles.Patient, ct);
        var nurses = await _db.Users.CountAsync(u => u.Role == AppRoles.Nurse, ct);
        var clinics = await _db.Users.CountAsync(u => u.Role == AppRoles.ClinicOwner, ct);
        var admins = await _db.Users.CountAsync(u => u.Role == AppRoles.Admin, ct);

        usersByRole.Add(new { Role = "مرضى", Count = patients });
        usersByRole.Add(new { Role = "ممرضين", Count = nurses });
        usersByRole.Add(new { Role = "عيادات", Count = clinics });
        usersByRole.Add(new { Role = "مديرين", Count = admins });

        // Appointments by day (last 30 days)
        var appointmentsByDay = await _db.Appointments
            .Where(a => a.CreatedAt >= today.AddDays(-30))
            .GroupBy(a => a.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync(ct);

        // Recent activity (from dashboard)
        var recentActivity = await _db.Appointments
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Take(20)
            .Select(a => new
            {
                Text = $"موعد جديد #{a.AppointmentId} — {a.Status}",
                At = a.CreatedAt,
                Type = "appointment"
            })
            .ToListAsync(ct);

        // Service popularity
        var servicePopularity = await _db.Appointments
            .Where(a => a.CreatedAt >= monthStart)
            .GroupBy(a => a.ServiceId)
            .Select(g => new
            {
                ServiceId = g.Key,
                ServiceName = g.First().Service != null ? g.First().Service.ServiceName : "خدمة غير محددة",
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync(ct);

        // Top rated nurses
        var topRatedNurses = await _db.NurseProfiles
            .AsNoTracking()
            .Include(n => n.User)
            .Where(n => n.TotalReviews > 0)
            .OrderByDescending(n => n.AverageRating)
            .Take(10)
            .Select(n => new
            {
                Name = n.User.FullName,
                Rating = n.AverageRating,
                Reviews = n.TotalReviews,
                Specialization = n.Specialization
            })
            .ToListAsync(ct);

        // Top rated clinics
        var topRatedClinics = await _db.Clinics
            .AsNoTracking()
            .Include(c => c.Owner)
            .Where(c => c.TotalReviews > 0)
            .OrderByDescending(c => c.AverageRating)
            .Take(10)
            .Select(c => new
            {
                Name = c.ClinicName,
                Rating = c.AverageRating,
                Reviews = c.TotalReviews,
                City = c.City
            })
            .ToListAsync(ct);

        // Revenue by service type
        var revenueByService = await _db.Appointments
            .Where(a => a.Status == AppointmentStatuses.Completed && a.CreatedAt >= monthStart)
            .GroupBy(a => a.ServiceId)
            .Select(g => new
            {
                ServiceName = g.First().Service != null ? g.First().Service.ServiceName : "خدمة غير محددة",
                Revenue = g.Sum(a => a.TotalPrice)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToListAsync(ct);

        // Contact messages stats
        var contactStats = new
        {
            Total = await _db.ContactMessages.CountAsync(ct),
            Unread = await _db.ContactMessages.CountAsync(m => !m.IsRead, ct),
            ThisMonth = await _db.ContactMessages.CountAsync(m => m.CreatedAt >= monthStart, ct)
        };

        // Article stats
        var articleStats = new
        {
            Total = await _db.Articles.CountAsync(ct),
            Published = await _db.Articles.CountAsync(a => a.IsPublished, ct),
            ThisMonth = await _db.Articles.CountAsync(a => a.CreatedAt >= monthStart, ct)
        };

        ViewBag.TotalUsers = totalUsers;
        ViewBag.TotalNurses = totalNurses;
        ViewBag.TotalClinics = totalClinics;
        ViewBag.AppointmentsToday = appointmentsToday;
        ViewBag.RevenueEstimate = revenueEstimate;
        ViewBag.PendingVerifications = pendingVerifications;
        ViewBag.StatusCounts = statusCounts;
        ViewBag.RevenueByMonth = revenueByMonth;
        ViewBag.UsersByRole = usersByRole;
        ViewBag.AppointmentsByDay = appointmentsByDay;
        ViewBag.RecentActivity = recentActivity;
        ViewBag.ServicePopularity = servicePopularity;
        ViewBag.TopRatedNurses = topRatedNurses;
        ViewBag.TopRatedClinics = topRatedClinics;
        ViewBag.RevenueByService = revenueByService;
        ViewBag.ContactStats = contactStats;
        ViewBag.ArticleStats = articleStats;

        return View();
    }

    private static string Slugify(string title)
    {
        var s = title.Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"\s+", "-", RegexOptions.None, TimeSpan.FromSeconds(1));
        s = Regex.Replace(s, @"[^a-z0-9\-]", "", RegexOptions.None, TimeSpan.FromSeconds(1));
        return s.Trim('-');
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug, int excludeId, CancellationToken ct)
    {
        var slug = baseSlug;
        var n = 0;
        while (await _db.Articles.AnyAsync(a => a.Slug == slug && a.ArticleId != excludeId, ct))
        {
            n++;
            slug = $"{baseSlug}-{n}";
        }
        return slug;
    }

    private async Task AdminDeleteClinicDataAsync(int clinicId, CancellationToken ct)
    {
        await _db.Appointments.Where(a => a.ClinicId == clinicId).ExecuteDeleteAsync(ct);
        await _db.Ratings.Where(r => r.ClinicId == clinicId).ExecuteDeleteAsync(ct);
        await _db.ClinicServices.Where(s => s.ClinicId == clinicId).ExecuteDeleteAsync(ct);
        await _db.ClinicWeeklySlots.Where(s => s.ClinicId == clinicId).ExecuteDeleteAsync(ct);
        await _db.Clinics.Where(c => c.ClinicId == clinicId).ExecuteDeleteAsync(ct);
    }

    private async Task AdminDeleteNurseDataAsync(int nurseProfileId, CancellationToken ct)
    {
        await _db.Appointments.Where(a => a.NurseProfileId == nurseProfileId).ExecuteDeleteAsync(ct);
        await _db.Ratings.Where(r => r.NurseProfileId == nurseProfileId).ExecuteDeleteAsync(ct);
        await _db.NurseServices.Where(ns => ns.NurseProfileId == nurseProfileId).ExecuteDeleteAsync(ct);
        await _db.NurseListingServices.Where(x => x.NurseProfileId == nurseProfileId).ExecuteDeleteAsync(ct);
        await _db.NurseWeeklySlots.Where(x => x.NurseProfileId == nurseProfileId).ExecuteDeleteAsync(ct);
        await _db.NurseProfiles.Where(n => n.NurseProfileId == nurseProfileId).ExecuteDeleteAsync(ct);
    }

    private async Task AdminDeleteUserContentAsync(string userId, CancellationToken ct)
    {
        await _db.Articles.Where(a => a.AuthorId == userId).ExecuteDeleteAsync(ct);
        await _db.Notifications.Where(n => n.UserId == userId).ExecuteDeleteAsync(ct);
        await _db.Appointments.Where(a => a.PatientId == userId).ExecuteDeleteAsync(ct);
    }
}
