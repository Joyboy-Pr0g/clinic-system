namespace HomeNursingSystem.Services;

public interface IFileUploadService
{
    /// <summary>Returns web-relative path starting with /uploads/ or null if invalid.</summary>
    Task<string?> SaveImageAsync(IFormFile file, string subFolder, CancellationToken ct = default);

    /// <summary>صورة أو تسجيل صوتي للدردشة (حد أقصى 15 ميجا).</summary>
    Task<string?> SaveChatAttachmentAsync(IFormFile file, CancellationToken ct = default);

    /// <summary>رخصة ممرض أو وثيقة عيادة: صورة أو PDF (حد أقصى 10 ميجا).</summary>
    Task<string?> SaveLicenseDocumentAsync(IFormFile file, CancellationToken ct = default);
}
