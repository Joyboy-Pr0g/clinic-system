using System.Diagnostics;
using HomeNursingSystem.Data;
using HomeNursingSystem.Services;
using HomeNursingSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Controllers;

public class HomeController : Controller
{
    private readonly INurseService _nurses;
    private readonly ApplicationDbContext _db;

    public HomeController(INurseService nurses, ApplicationDbContext db)
    {
        _nurses = nurses;
        _db = db;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var featured = await _nurses.BrowseAsync(null, null, 4, true, null, ct);
        var services = await _db.Services.AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.ServiceName)
            .Select(s => new ServiceListItemVM
            {
                ServiceId = s.ServiceId,
                ServiceName = s.ServiceName,
                BasePrice = s.BasePrice
            })
            .Take(8)
            .ToListAsync(ct);

        var vm = new LandingVM
        {
            FeaturedNurses = featured.Take(4).ToList(),
            Services = services,
            NursesCount = await _db.NurseProfiles.CountAsync(n => n.IsVerified, ct),
            ClinicsCount = await _db.Clinics.CountAsync(c => c.IsVerified, ct),
            AppointmentsCount = await _db.Appointments.CountAsync(ct)
        };
        return View(vm);
    }

    public IActionResult About() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
