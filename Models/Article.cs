namespace HomeNursingSystem.Models;

public class Article
{
    public int ArticleId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public ApplicationUser Author { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ThumbnailImagePath { get; set; }
    public string Category { get; set; } = ArticleCategories.HealthTips;
    public bool IsNews { get; set; }
    public bool IsPublished { get; set; }
    public int ViewCount { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
