using HomeNursingSystem.Data;
using HomeNursingSystem.Data.Repositories;
using HomeNursingSystem.Models;
using HomeNursingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Controllers;

[Authorize(Roles = AppRoles.Nurse)]
[Route("nurse/services")]
public class NurseServicesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly INurseProfileRepository _profiles;
    private readonly UserManager<ApplicationUser> _users;

    public NurseServicesController(
        ApplicationDbContext db,
        INurseProfileRepository profiles,
        UserManager<ApplicationUser> users)
    {
        _db = db;
        _profiles = profiles;
        _users = users;
    }

    private async Task<NurseProfile?> GetVerifiedProfileAsync(CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return null;
        var np = await _profiles.GetByUserIdAsync(user.Id, ct);
        if (np == null || !np.IsVerified) return null;
        return np;
    }

    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var np = await GetVerifiedProfileAsync(ct);
        if (np == null) return RedirectToAction("PendingApproval", "Account");

        var list = await _db.NurseListingServices.AsNoTracking()
            .Where(s => s.NurseProfileId == np.NurseProfileId)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
        return View("~/Views/Nurse/NurseServices.cshtml", list);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var np = await GetVerifiedProfileAsync(ct);
        if (np == null) return RedirectToAction("PendingApproval", "Account");

        return View("~/Views/Nurse/NurseServiceEdit.cshtml", new NurseListingServiceEditVM());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NurseListingServiceEditVM model, CancellationToken ct)
    {
        var np = await GetVerifiedProfileAsync(ct);
        if (np == null) return NotFound();

        if (!ModelState.IsValid)
            return View("~/Views/Nurse/NurseServiceEdit.cshtml", model);

        _db.NurseListingServices.Add(new NurseListingService
        {
            NurseProfileId = np.NurseProfileId,
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
        var np = await GetVerifiedProfileAsync(ct);
        if (np == null) return NotFound();

        var row = await _db.NurseListingServices.FirstOrDefaultAsync(
            s => s.NurseListingServiceId == id && s.NurseProfileId == np.NurseProfileId, ct);
        if (row == null) return NotFound();

        var vm = new NurseListingServiceEditVM
        {
            NurseListingServiceId = row.NurseListingServiceId,
            Name = row.Name,
            Price = row.Price
        };
        return View("~/Views/Nurse/NurseServiceEdit.cshtml", vm);
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NurseListingServiceEditVM model, CancellationToken ct)
    {
        var np = await GetVerifiedProfileAsync(ct);
        if (np == null) return NotFound();

        var row = await _db.NurseListingServices.FirstOrDefaultAsync(
            s => s.NurseListingServiceId == id && s.NurseProfileId == np.NurseProfileId, ct);
        if (row == null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.NurseListingServiceId = id;
            return View("~/Views/Nurse/NurseServiceEdit.cshtml", model);
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
        var np = await GetVerifiedProfileAsync(ct);
        if (np == null) return NotFound();

        var row = await _db.NurseListingServices.FirstOrDefaultAsync(
            s => s.NurseListingServiceId == id && s.NurseProfileId == np.NurseProfileId, ct);
        if (row == null) return NotFound();

        _db.NurseListingServices.Remove(row);
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تم حذف الخدمة.";
        return RedirectToAction(nameof(Index));
    }
}
