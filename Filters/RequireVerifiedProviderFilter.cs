using HomeNursingSystem.Data;
using HomeNursingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Filters;

public class RequireVerifiedProviderFilter : IAsyncActionFilter
{
    private readonly ApplicationDbContext _db;

    public RequireVerifiedProviderFilter(ApplicationDbContext db) => _db = db;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor.EndpointMetadata.Any(m => m is AllowUnverifiedAttribute))
        {
            await next();
            return;
        }

        var user = context.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            await next();
            return;
        }

        var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            await next();
            return;
        }

        var ctrl = context.RouteData.Values["controller"]?.ToString();
        var act = context.RouteData.Values["action"]?.ToString();
        if (ctrl == "Account" && act is "PendingApproval" or "Logout" or "Login" or "Register" or "RegisterNurse" or "RegisterClinic")
        {
            await next();
            return;
        }

        if (user.IsInRole(AppRoles.Nurse))
        {
            var np = await _db.NurseProfiles.AsNoTracking().FirstOrDefaultAsync(n => n.UserId == userId);
            if (np == null || !np.IsVerified)
            {
                context.Result = new RedirectToActionResult("PendingApproval", "Account", null);
                return;
            }
        }
        else if (user.IsInRole(AppRoles.ClinicOwner))
        {
            var clinic = await _db.Clinics.AsNoTracking().FirstOrDefaultAsync(c => c.OwnerId == userId);
            if (clinic == null || !clinic.IsVerified)
            {
                context.Result = new RedirectToActionResult("PendingApproval", "Account", null);
                return;
            }
        }

        await next();
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AllowUnverifiedAttribute : Attribute
{
}
