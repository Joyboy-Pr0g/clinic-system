using System.ComponentModel.DataAnnotations;

namespace HomeNursingSystem.ViewModels;

public class ArticleVM
{
    public int? ArticleId { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "العنوان")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "ملخص")]
    public string? Summary { get; set; }

    [Required]
    [Display(Name = "المحتوى")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "الفئة")]
    public string Category { get; set; } = Models.ArticleCategories.HealthTips;

    [Display(Name = "خبر")]
    public bool IsNews { get; set; }

    [Display(Name = "منشور")]
    public bool IsPublished { get; set; }

    public IFormFile? ThumbnailFile { get; set; }
    public string? ThumbnailImagePath { get; set; }
    public string? Slug { get; set; }
}
