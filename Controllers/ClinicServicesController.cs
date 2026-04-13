using HomeNursingSystem.Data;
using HomeNursingSystem.Data.Repositories;
using HomeNursingSystem.Models;
using HomeNursingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Controllers;

[Authorize(Roles = AppRoles.ClinicOwner)]
[Route("clinic/services")]
public class ClinicServicesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IClinicRepository _clinics;
    private readonly UserManager<ApplicationUser> _users;

    public ClinicServicesController(
        ApplicationDbContext db,
        IClinicRepository clinics,
        UserManager<ApplicationUser> users)
    {
        _db = db;
        _clinics = clinics;
        _users = users;
    }

    private async Task<Clinic?> GetOwnedClinicAsync(CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return null;
        return await _clinics.GetByOwnerIdAsync(user.Id, ct);
    }

    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var clinic = await GetOwnedClinicAsync(ct);
        if (clinic == null) return RedirectToAction("PendingApproval", "Account");

        var list = await _db.ClinicServices.AsNoTracking()
            .Where(s => s.ClinicId == clinic.ClinicId)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
        return View("~/Views/Clinic/ClinicServices.cshtml", list);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View("~/Views/Clinic/ClinicServiceEdit.cshtml", new ClinicServiceEditVM());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClinicServiceEditVM model, CancellationToken ct)
    {
        var clinic = await GetOwnedClinicAsync(ct);
        if (clinic == null) return NotFound();

        if (!ModelState.IsValid)
            return View("~/Views/Clinic/ClinicServiceEdit.cshtml", model);

        _db.ClinicServices.Add(new ClinicService
        {
            ClinicId = clinic.ClinicId,
            Name = model.Name.Trim(),
            Price = model.Price
        });
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تمت إضافة الخدمة.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var clinic = await GetOwnedClinicAsync(ct);
        if (clinic == null) return NotFound();

        var row = await _db.ClinicServices.FirstOrDefaultAsync(
            s => s.ClinicServiceId == id && s.ClinicId == clinic.ClinicId, ct);
        if (row == null) return NotFound();

        var vm = new ClinicServiceEditVM
        {
            ClinicServiceId = row.ClinicServiceId,
            Name = row.Name,
            Price = row.Price
        };
        return View("~/Views/Clinic/ClinicServiceEdit.cshtml", vm);
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClinicServiceEditVM model, CancellationToken ct)
    {
        var clinic = await GetOwnedClinicAsync(ct);
        if (clinic == null) return NotFound();

        var row = await _db.ClinicServices.FirstOrDefaultAsync(
            s => s.ClinicServiceId == id && s.ClinicId == clinic.ClinicId, ct);
        if (row == null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.ClinicServiceId = id;
            return View("~/Views/Clinic/ClinicServiceEdit.cshtml", model);
        }

        row.Name = model.Name.Trim();
        row.Price = model.Price;
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تم تحديث الخدمة.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var clinic = await GetOwnedClinicAsync(ct);
        if (clinic == null) return NotFound();

        var row = await _db.ClinicServices.FirstOrDefaultAsync(
            s => s.ClinicServiceId == id && s.ClinicId == clinic.ClinicId, ct);
        if (row == null) return NotFound();

        _db.ClinicServices.Remove(row);
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تم حذف الخدمة.";
        return RedirectToAction(nameof(Index));
    }
}
