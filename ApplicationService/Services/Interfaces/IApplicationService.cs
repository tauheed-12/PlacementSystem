using ApplicationService.Entities;
using ApplicationService.DTO;

namespace ApplicationService.Services.Interfaces
{
    public interface IApplicationService
    {
        public Task ApplyAsync(ApplicationRequestDto application, CancellationToken cancellationToken);
        public Task DeleteApplication(Guid applicationId, CancellationToken cancellationToken);
        public Task<List<UserApplicationsResponseDto>> GetUsersApplications(Guid studentId, CancellationToken cancellationToken);
        public Task<List<Application>> GetDriveApplications(Guid driveId, CancellationToken cancellationToken);
        // public Task<Application> GetApplication(Guid applicationId, CancellationToken cancellationToken);
    }
}
