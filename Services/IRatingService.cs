namespace HomeNursingSystem.Services;

public interface IRatingService
{
    Task<(bool ok, string? error)> SubmitRatingAsync(string patientId, int appointmentId, int stars, string? comment, CancellationToken ct = default);
}
