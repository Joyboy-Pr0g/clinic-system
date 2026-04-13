using HomeNursingSystem.Data;
using HomeNursingSystem.Models;
using HomeNursingSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HomeNursingSystem.Controllers;

public class ContactController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public ContactController(ApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpGet("/contact")]
    public IActionResult Index()
    {
        var key = _config["GoogleMapsApiKey"];
        var place = _config["GoogleMapsContactPlaceQuery"] ?? "Riyadh Saudi Arabia";
        if (!string.IsNullOrEmpty(key) && key != "YOUR_GOOGLE_MAPS_API_KEY")
        {
            ViewBag.MapEmbedUrl =
                "https://www.google.com/maps/embed/v1/place?key=" + Uri.EscapeDataString(key)
                + "&q=" + Uri.EscapeDataString(place);
        }

        return View(new ContactVM());
    }

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
