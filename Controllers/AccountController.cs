using HomeNursingSystem.Data;
using HomeNursingSystem.Models;
using HomeNursingSystem.Services;
using HomeNursingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _db;
    private readonly IFileUploadService _files;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext db,
        IFileUploadService files)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _files = files;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginVM { ReturnUrl = returnUrl });

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "بيانات الدخول غير صحيحة.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "بيانات الدخول غير صحيحة.");
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return await RedirectToDashboardAsync(user);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View(new RegisterVM());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVM model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (model.SelectedRole != AppRoles.Patient)
        {
            if (model.SelectedRole == AppRoles.Nurse)
                return RedirectToAction(nameof(RegisterNurse));
            if (model.SelectedRole == AppRoles.ClinicOwner)
                return RedirectToAction(nameof(RegisterClinic));
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            Role = AppRoles.Patient,
            City = model.City,
            Neighborhood = model.Neighborhood,
            Street = model.Street,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, AppRoles.Patient);
        await _signInManager.SignInAsync(user, isPersistent: true);
        return RedirectToAction("Dashboard", "Patient");
    }

    private async Task<IActionResult> RedirectToDashboardAsync(ApplicationUser user)
    {
        if (await _userManager.IsInRoleAsync(user, AppRoles.Admin))
            return RedirectToAction("Dashboard", "Admin");
        if (await _userManager.IsInRoleAsync(user, AppRoles.Nurse))
            return RedirectToAction("Dashboard", "Nurse");
        if (await _userManager.IsInRoleAsync(user, AppRoles.ClinicOwner))
            return RedirectToAction("Dashboard", "Clinic");
        return RedirectToAction("Dashboard", "Patient");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterNurse() => View(new RegisterNursePageVM());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(12 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 12 * 1024 * 1024)]
    public async Task<IActionResult> RegisterNurse(RegisterNursePageVM model, CancellationToken ct)
    {
        if (model.LicenseFile == null || model.LicenseFile.Length == 0)
            ModelState.AddModelError(nameof(model.LicenseFile), "يجب إرفاق التصريح الطبي أو رخصة المزاولة (صورة أو PDF) للتحقق من هويتك كممرض.");

        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            Role = AppRoles.Nurse,
            City = model.City,
            Neighborhood = model.Neighborhood,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, AppRoles.Nurse);

        var licensePath = await _files.SaveLicenseDocumentAsync(model.LicenseFile!, ct);
        if (licensePath == null)
        {
            await _userManager.DeleteAsync(user);
            ModelState.AddModelError(nameof(model.LicenseFile), "الملف غير مدعوم (صورة أو PDF فقط) أو يتجاوز 10 ميجابايت.");
            return View(model);
        }

        _db.NurseProfiles.Add(new NurseProfile
        {
            UserId = user.Id,
            Specialization = model.Specialization,
            YearsOfExperience = model.YearsOfExperience,
            Bio = model.Bio,
            LicenseImagePath = licensePath,
            IsVerified = false,
            IsRejectedByAdmin = false,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);

        await _signInManager.SignInAsync(user, isPersistent: true);
        return RedirectToAction(nameof(PendingApproval));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterClinic() => View(new RegisterClinicPageVM());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(12 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 12 * 1024 * 1024)]
    public async Task<IActionResult> RegisterClinic(RegisterClinicPageVM model, CancellationToken ct)
    {
        if (model.LicenseDocumentFile == null || model.LicenseDocumentFile.Length == 0)
            ModelState.AddModelError(nameof(model.LicenseDocumentFile), "يجب إرفاق رخصة المنشأة أو التصريح الطبي أو السجل التجاري (صورة أو PDF).");

        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            FullName = model.OwnerFullName,
            PhoneNumber = model.PhoneNumber,
            Role = AppRoles.ClinicOwner,
            City = model.City,
            Neighborhood = model.Neighborhood,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, AppRoles.ClinicOwner);

        string? logo = null;
        if (model.LogoFile != null)
            logo = await _files.SaveImageAsync(model.LogoFile, "clinics", ct);

        var licensePath = await _files.SaveLicenseDocumentAsync(model.LicenseDocumentFile!, ct);
        if (licensePath == null)
        {
            await _userManager.DeleteAsync(user);
            ModelState.AddModelError(nameof(model.LicenseDocumentFile), "وثيقة الرخصة غير مدعومة أو تتجاوز 10 ميجابايت.");
            return View(model);
        }

        _db.Clinics.Add(new Clinic
        {
            OwnerId = user.Id,
            ClinicName = model.ClinicName,
            Address = model.Address,
            Neighborhood = model.Neighborhood,
            City = model.City,
            Latitude = 24.7136,
            Longitude = 46.6753,
            PhoneNumber = model.ClinicPhone,
            Email = model.ClinicEmail,
            LogoImagePath = logo,
            LicenseDocumentPath = licensePath,
            IsVerified = false,
            IsRejectedByAdmin = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);

        await _signInManager.SignInAsync(user, isPersistent: true);
        return RedirectToAction(nameof(PendingApproval));
    }

    [Authorize]
    public async Task<IActionResult> PendingApproval(CancellationToken ct)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));
        if (user.Role == AppRoles.Nurse)
        {
            var np = await _db.NurseProfiles.AsNoTracking().FirstOrDefaultAsync(n => n.UserId == user.Id, ct);
            if (np?.IsRejectedByAdmin == true)
                return RedirectToAction(nameof(VerificationRejected));
        }
        else if (user.Role == AppRoles.ClinicOwner)
        {
            var c = await _db.Clinics.AsNoTracking().FirstOrDefaultAsync(x => x.OwnerId == user.Id, ct);
            if (c?.IsRejectedByAdmin == true)
                return RedirectToAction(nameof(VerificationRejected));
        }
        return View();
    }

    [Authorize]
    public async Task<IActionResult> VerificationRejected(CancellationToken ct)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));
        if (user.Role == AppRoles.Nurse)
        {
            var np = await _db.NurseProfiles.AsNoTracking().FirstOrDefaultAsync(n => n.UserId == user.Id, ct);
            if (np == null || !np.IsRejectedByAdmin)
                return RedirectToAction(nameof(PendingApproval));
            ViewBag.RejectionNote = np.AdminRejectionNote;
            return View();
        }
        if (user.Role == AppRoles.ClinicOwner)
        {
            var c = await _db.Clinics.AsNoTracking().FirstOrDefaultAsync(x => x.OwnerId == user.Id, ct);
            if (c == null || !c.IsRejectedByAdmin)
                return RedirectToAction(nameof(PendingApproval));
            ViewBag.RejectionNote = c.AdminRejectionNote;
            return View();
        }
        return RedirectToAction("Index", "Home");
    }

    /// <summary>يُستدعى من المتصفح بعد تسجيل الدخول لتحديث آخر موقع (مريض/ممرض معتمد) لاستخدامه في روابط الخرائط.</summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveLiveLocation(double lat, double lng, CancellationToken ct)
    {
        if (lat is < -90 or > 90 || lng is < -180 or > 180)
            return BadRequest();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        user.Latitude = lat;
        user.Longitude = lng;
        user.LastLiveLocationAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var np = await _db.NurseProfiles.FirstOrDefaultAsync(n => n.UserId == user.Id, ct);
        if (np is { IsVerified: true })
        {
            np.LastLatitude = lat;
            np.LastLongitude = lng;
            np.LocationUpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return Ok();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
