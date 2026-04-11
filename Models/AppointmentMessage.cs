namespace HomeNursingSystem.Models;

public class AppointmentMessage
{
    public int AppointmentMessageId { get; set; }
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public string SenderUserId { get; set; } = string.Empty;
    public ApplicationUser Sender { get; set; } = null!;
    public string Body { get; set; } = string.Empty;
    /// <summary>text | image | audio</summary>
    public string MessageType { get; set; } = "text";
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
