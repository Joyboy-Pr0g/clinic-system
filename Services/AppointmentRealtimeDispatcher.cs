using HomeNursingSystem.Hubs;
using HomeNursingSystem.ViewModels;
using Microsoft.AspNetCore.SignalR;

namespace HomeNursingSystem.Services;

public class AppointmentRealtimeDispatcher : IAppointmentRealtimeDispatcher
{
    private readonly IHubContext<AppointmentHub> _hub;

    public AppointmentRealtimeDispatcher(IHubContext<AppointmentHub> hub) => _hub = hub;

    public Task BroadcastChatMessageAsync(int appointmentId, AppointmentChatMessageDto dto, CancellationToken ct = default) =>
        _hub.Clients.Group(AppointmentHub.GroupName(appointmentId)).SendAsync("ReceiveChatMessage", dto, ct);

    public Task BroadcastNurseLocationAsync(int appointmentId, double latitude, double longitude, CancellationToken ct = default) =>
        _hub.Clients.Group(AppointmentHub.GroupName(appointmentId)).SendAsync("NurseLocationUpdated", latitude, longitude, ct);

    public Task BroadcastPatientLocationAsync(int appointmentId, double latitude, double longitude, CancellationToken ct = default) =>
        _hub.Clients.Group(AppointmentHub.GroupName(appointmentId)).SendAsync("PatientLocationUpdated", latitude, longitude, ct);
}
