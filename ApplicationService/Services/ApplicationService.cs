using ApplicationService.Data.Interfaces;
using ApplicationService.Repositories.Interfaces;
using ApplicationService.Services.Interfaces;
using ApplicationService.Entities;
using ApplicationService.DTO;
using ApplicationService.HttpClients.Interfaces;
using ApplicationService.Exceptions;

namespace ApplicationService.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPlacementDriveServiceClient _placementDriveServiceClient;

        public ApplicationService(IUnitOfWork unitOfWork, IApplicationRepository repository, 
            IPlacementDriveServiceClient placementDriveServiceClient)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _placementDriveServiceClient = placementDriveServiceClient;
        }

        public async Task ApplyAsync(ApplicationRequestDto request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (request.DriveId == Guid.Empty || request.StudentId == Guid.Empty)
            {
                throw new ArgumentException("DriveId and StudentId are required.");
            }

            var existing = await _repository.GetByStudentIdAsync(request.StudentId, cancellationToken);
            if (existing.Any(a => a.DriveId == request.DriveId))
            {
                throw new InvalidOperationException("You have already applied to this drive.");
            }

            var application = Application.Create(request.StudentId, request.DriveId);

            await _repository.AddAsync(application, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteApplication(Guid applicationId, Guid studentId, CancellationToken cancellationToken)
        {
            var application = await _repository.GetByApplicationIdAsync(applicationId, cancellationToken);
            if (application == null)
            {
                throw new NotFoundException("Application not found");
            }
            if (application.StudentUserId != studentId)
            {
                throw new UnauthorizedAccessException("You can only withdraw your own applications.");
            }
            _repository.Remove(application);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<UserApplicationsResponseDto>> GetUsersApplications(Guid studentId, CancellationToken cancellationToken)
        {
            var applications = await _repository.GetByStudentIdAsync(studentId, cancellationToken);
            if (applications == null || applications.Count() == 0)
            {
                return new List<UserApplicationsResponseDto>();
            }

            var driveIds = applications.Select(x => x.DriveId).Distinct().ToList();

            var drives = await _placementDriveServiceClient.GetDrivesBulkAsync(driveIds, cancellationToken);
            
            return applications
                .Where(a => drives.ContainsKey(a.DriveId))
                .Select( a =>
                {
                    var drive = drives[a.DriveId];
                    return new UserApplicationsResponseDto
                    {
                        ApplicationId = a.Id,
                        CompanyName = drive.CompanyName,
                        Status = a.Status.ToString(),
                        DriveDate = drive.DriveDate,
                        AppliedAt = a.AppliedAt
                    };
                })
                .ToList();
        }

        // TODO: Update below api logic
        public async Task<List<Application>> GetDriveApplications(Guid driveId, CancellationToken cancellationToken)
        {
            return await _repository.GetByDriveIdAsync(driveId, cancellationToken);
        }
    }
}
