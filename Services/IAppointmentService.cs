using HomeNursingSystem.ViewModels;

namespace HomeNursingSystem.Services;

public interface IAppointmentService
{
    Task<PatientDashboardVM> GetPatientDashboardAsync(string patientId, CancellationToken ct = default);
    Task<NurseDashboardVM> GetNurseDashboardAsync(int nurseProfileId, CancellationToken ct = default);
    Task<ClinicDashboardVM> GetClinicDashboardAsync(int clinicId, CancellationToken ct = default);
    Task<AdminDashboardVM> GetAdminDashboardAsync(CancellationToken ct = default);
    Task<(bool ok, string? error, int? appointmentId)> BookAsync(AppointmentBookVM model, string patientId, CancellationToken ct = default);
    Task<(bool ok, string? error)> UpdateStatusAsync(int appointmentId, string status, string actorUserId, string role, CancellationToken ct = default);
}
