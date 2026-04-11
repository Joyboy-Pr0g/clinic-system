using HomeNursingSystem.ViewModels;

namespace HomeNursingSystem.Services;

public interface IAppointmentRealtimeDispatcher
{
    Task BroadcastChatMessageAsync(int appointmentId, AppointmentChatMessageDto dto, CancellationToken ct = default);
    Task BroadcastNurseLocationAsync(int appointmentId, double latitude, double longitude, CancellationToken ct = default);
    Task BroadcastPatientLocationAsync(int appointmentId, double latitude, double longitude, CancellationToken ct = default);
}
