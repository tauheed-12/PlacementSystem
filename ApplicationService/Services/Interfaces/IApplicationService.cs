using ApplicationService.DTO;
using ApplicationService.Entities;
using System.Threading;

namespace ApplicationService.Services.Interfaces
{
    public interface IApplicationService
    {
        public Task ApplyAsync(ApplicationRequestDto application, CancellationToken cancellationToken);
        public Task DeleteApplication(Guid applicationId, Guid studentId, CancellationToken cancellationToken);
        public Task<List<UserApplicationsResponseDto>> GetUsersApplications(Guid studentId, CancellationToken cancellationToken);
        public Task<List<Application>> GetDriveApplications(Guid driveId, CancellationToken cancellationToken);
        public Task<List<StudentApplicationDto>> GetStudentApplication(Guid studentId, CancellationToken cancellationToken);
        // public Task<Application> GetApplication(Guid applicationId, CancellationToken cancellationToke
    }
}
