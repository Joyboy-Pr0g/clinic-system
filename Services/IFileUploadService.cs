namespace HomeNursingSystem.Services;

public interface IFileUploadService
{
    /// <summary>Returns web-relative path starting with /uploads/ or null if invalid.</summary>
    Task<string?> SaveImageAsync(IFormFile file, string subFolder, CancellationToken ct = default);
}
