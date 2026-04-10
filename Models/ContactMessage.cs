namespace HomeNursingSystem.Models;

public class ContactMessage
{
    public int ContactMessageId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? RepliedAt { get; set; }
    public string? ReplyMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
