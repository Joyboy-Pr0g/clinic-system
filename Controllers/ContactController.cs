using HomeNursingSystem.Data;
using HomeNursingSystem.Models;
using HomeNursingSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HomeNursingSystem.Controllers;

public class ContactController : Controller
{
    private readonly ApplicationDbContext _db;

    public ContactController(ApplicationDbContext db) => _db = db;

    [HttpGet("/contact")]
    public IActionResult Index() => View(new ContactVM());

    [HttpPost("/contact")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactVM model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);
        _db.ContactMessages.Add(new ContactMessage
        {
            FullName = model.FullName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Subject = model.Subject,
            Message = model.Message,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
        TempData["Success"] = "تم استلام رسالتك، سنتواصل قريباً.";
        return RedirectToAction(nameof(Index));
    }
}
