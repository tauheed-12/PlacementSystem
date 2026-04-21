namespace DashboardOrchestrationService.Clients.Interfaces;

using DashboardOrchestrationService.DTOs;

public interface IApplicationServiceClient
{
    Task<List<ApplicationStatusDto>> GetApplicationsAsync(Guid studentId);
}