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
    public async Task<IActionResult> RegisterNurse(RegisterNursePageVM model)
    {
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

        string? licensePath = null;
        if (model.LicenseFile != null)
            licensePath = await _files.SaveImageAsync(model.LicenseFile, "licenses");

        _db.NurseProfiles.Add(new NurseProfile
        {
            UserId = user.Id,
            Specialization = model.Specialization,
            YearsOfExperience = model.YearsOfExperience,
            Bio = model.Bio,
            LicenseImagePath = licensePath,
            IsVerified = false,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        await _signInManager.SignInAsync(user, isPersistent: true);
        return RedirectToAction(nameof(PendingApproval));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterClinic() => View(new RegisterClinicPageVM());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterClinic(RegisterClinicPageVM model)
    {
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
            logo = await _files.SaveImageAsync(model.LogoFile, "clinics");

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
            IsVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        await _signInManager.SignInAsync(user, isPersistent: true);
        return RedirectToAction(nameof(PendingApproval));
    }

    [Authorize]
    public IActionResult PendingApproval() => View();

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
