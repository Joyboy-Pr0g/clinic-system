using HomeNursingSystem.Data;
using HomeNursingSystem.Models;
using HomeNursingSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeNursingSystem.Controllers;

public class ArticleController : Controller
{
    private readonly ApplicationDbContext _db;

    public ArticleController(ApplicationDbContext db) => _db = db;

    [HttpGet("/articles")]
    public async Task<IActionResult> Index(string? category, CancellationToken ct)
    {
        ViewBag.Category = category;
        var q = _db.Articles.AsNoTracking().Where(a => a.IsPublished);
        if (!string.IsNullOrEmpty(category))
            q = q.Where(a => a.Category == category);
        var list = await q
            .OrderByDescending(a => a.PublishedAt)
            .Select(a => new ArticleListItemVM
            {
                ArticleId = a.ArticleId,
                Title = a.Title,
                Slug = a.Slug,
                Summary = a.Summary,
                ThumbnailImagePath = a.ThumbnailImagePath,
                Category = a.Category,
                IsNews = a.IsNews,
                PublishedAt = a.PublishedAt
            })
            .ToListAsync(ct);
        return View(list);
    }

    [HttpGet("/articles/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken ct)
    {
        var article = await _db.Articles.FirstOrDefaultAsync(a => a.Slug == slug && a.IsPublished, ct);
        if (article == null) return NotFound();
        article.ViewCount++;
        await _db.SaveChangesAsync(ct);

        var related = await _db.Articles.AsNoTracking()
            .Where(a => a.IsPublished && a.Category == article.Category && a.ArticleId != article.ArticleId)
            .OrderByDescending(a => a.PublishedAt)
            .Take(4)
            .Select(a => new ArticleListItemVM
            {
                ArticleId = a.ArticleId,
                Title = a.Title,
                Slug = a.Slug,
                Summary = a.Summary,
                ThumbnailImagePath = a.ThumbnailImagePath,
                Category = a.Category,
                IsNews = a.IsNews,
                PublishedAt = a.PublishedAt
            })
            .ToListAsync(ct);

        var vm = new ArticleDetailsVM
        {
            ArticleId = article.ArticleId,
            Title = article.Title,
            Slug = article.Slug,
            Content = article.Content,
            ThumbnailImagePath = article.ThumbnailImagePath,
            Category = article.Category,
            ViewCount = article.ViewCount,
            PublishedAt = article.PublishedAt,
            Related = related
        };
        return View(vm);
    }
}
