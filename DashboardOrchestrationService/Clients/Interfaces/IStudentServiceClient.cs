// Clients/Interfaces/IStudentServiceClient.cs
using DashboardOrchestrationService.DTOs;

namespace DashboardOrchestrationService.Clients.Interfaces
{
    public interface IStudentServiceClient
    {
        Task<StudentProfileDto> GetStudentProfileById(Guid studentId);
    }
}