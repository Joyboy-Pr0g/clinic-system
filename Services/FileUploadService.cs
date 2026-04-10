using Microsoft.AspNetCore.Hosting;

namespace HomeNursingSystem.Services;

public class FileUploadService : IFileUploadService
{
    private const long MaxBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private readonly IWebHostEnvironment _env;

    public FileUploadService(IWebHostEnvironment env) => _env = env;

    public async Task<string?> SaveImageAsync(IFormFile file, string subFolder, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0 || file.Length > MaxBytes)
            return null;

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !Allowed.Contains(ext))
            return null;

        var safeFolder = subFolder.Trim().Replace("..", "").Trim('/');
        var uploads = Path.Combine(_env.WebRootPath, "uploads", safeFolder);
        Directory.CreateDirectory(uploads);

        var name = $"{Guid.NewGuid():N}{ext}";
        var physical = Path.Combine(uploads, name);
        await using (var stream = File.Create(physical))
            await file.CopyToAsync(stream, ct);

        return $"/uploads/{safeFolder}/{name}".Replace('\\', '/');
    }
}
