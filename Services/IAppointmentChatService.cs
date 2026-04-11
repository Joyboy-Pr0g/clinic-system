using HomeNursingSystem.ViewModels;

namespace HomeNursingSystem.Services;

public interface IAppointmentChatService
{
    Task<IReadOnlyList<AppointmentChatMessageDto>> GetMessagesAsync(int appointmentId, string userId, int? afterMessageId, CancellationToken ct = default);
    Task<(bool ok, string? error)> SendAsync(int appointmentId, string userId, string? body, string? attachmentUrl, CancellationToken ct = default);
}
