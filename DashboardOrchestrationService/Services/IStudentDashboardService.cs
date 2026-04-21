using DashboardOrchestrationService.DTOs;

namespace DashboardOrchestrationService.Services;

public interface IStudentDashboardService
{
    Task<StudentDashboardDto> GetStudentDashboardAsync(Guid studentId, CancellationToken ct);
}