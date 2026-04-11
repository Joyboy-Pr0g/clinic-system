namespace HomeNursingSystem.ViewModels;

public class AppointmentChatMessageDto
{
    public int AppointmentMessageId { get; set; }
    public string SenderUserId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string MessageType { get; set; } = "text";
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsMine { get; set; }
}
