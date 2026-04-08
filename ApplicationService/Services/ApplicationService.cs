using ApplicationService.Data.Interfaces;
using ApplicationService.Repositories.Interfaces;
using ApplicationService.Services.Interfaces;
using ApplicationService.Entities;
using ApplicationService.HttpClients.Interfaces;
using static ApplicationService.DTO.Dtos;
using ApplicationService.Middleware;

namespace ApplicationService.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPlacementDriveServiceClient _placementDriveServiceClient;

        public ApplicationService(
            IUnitOfWork unitOfWork,
            IApplicationRepository repository,
            IPlacementDriveServiceClient placementDriveServiceClient)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _placementDriveServiceClient = placementDriveServiceClient;
        }

        // ---------------- APPLY ----------------
        public async Task ApplyAsync(CreateApplicationRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ValidationException("Request cannot be null");

            if (request.DriveId == Guid.Empty)
                throw new ValidationException("DriveId is required");

            if (request.StudentId == Guid.Empty)
                throw new ValidationException("StudentId is required");

            var drives = await _placementDriveServiceClient
                .GetDrivesBulkAsync(new List<Guid> { request.DriveId }, cancellationToken);

            if (!drives.ContainsKey(request.DriveId))
                throw new NotFoundException("Drive not found");

            var drive = drives[request.DriveId];

            if (drive.ApplicationDeadline < DateTime.UtcNow)
                throw new ValidationException("Application deadline has passed");

            var alreadyExists = await _repository.ExistsAsync(request.StudentId, request.DriveId, cancellationToken);

            if (alreadyExists)
                throw new ConflictException("You have already applied to this drive");

            var application = Application.Create(request.StudentId, request.DriveId);

            await _repository.AddAsync(application, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // ---------------- DELETE ----------------
        public async Task DeleteApplicationAsync(Guid applicationId, Guid studentId, CancellationToken cancellationToken)
        {
            if (applicationId == Guid.Empty)
                throw new ValidationException("ApplicationId is required");

            if (studentId == Guid.Empty)
                throw new ValidationException("StudentId is required");

            var application = await _repository.GetByApplicationIdAsync(applicationId, cancellationToken);

            if (application == null)
                throw new NotFoundException("Application not found");

            if (application.StudentUserId != studentId)
                throw new ForbiddenException("You can only withdraw your own applications");

            var drives = await _placementDriveServiceClient
                .GetDrivesBulkAsync(new List<Guid> { application.DriveId }, cancellationToken);

            if (drives.ContainsKey(application.DriveId))
            {
                var drive = drives[application.DriveId];

                if (drive.ApplicationDeadline < DateTime.UtcNow)
                    throw new ValidationException("Cannot withdraw application after deadline");
            }

            _repository.Remove(application);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // ---------------- USER APPLICATIONS ----------------
        public async Task<List<UserApplicationSummary>> GetUserApplicationsAsync(
            Guid studentId,
            CancellationToken cancellationToken)
        {
            if (studentId == Guid.Empty)
                throw new ValidationException("Invalid student ID");

            var applications = await _repository.GetByStudentIdAsync(studentId, cancellationToken);

            if (!applications.Any())
                return new List<UserApplicationSummary>();

            var driveIds = applications.Select(x => x.DriveId).Distinct().ToList();

            var drives = await _placementDriveServiceClient
                .GetDrivesBulkAsync(driveIds, cancellationToken);

            return applications
                .Where(a => drives.ContainsKey(a.DriveId))
                .Select(a =>
                {
                    var drive = drives[a.DriveId];

                    return new UserApplicationSummary(
                        a.Id,
                        drive.CompanyName,
                        a.Status.ToString(),
                        a.AppliedAt,
                        drive.DriveDate
                    );
                })
                .ToList();
        }

        // ---------------- STUDENT APPLICATIONS ----------------
        public async Task<List<StudentApplication>> GetStudentApplicationsAsync(
            Guid studentId,
            CancellationToken cancellationToken)
        {
            if (studentId == Guid.Empty)
                throw new ValidationException("Invalid student ID");

            var applications = await _repository.GetByStudentIdAsync(studentId, cancellationToken);

            if (!applications.Any())
                return new List<StudentApplication>();

            return applications
                .Select(a => new StudentApplication(
                    a.Id,
                    a.DriveId,
                    a.AppliedAt,
                    a.Status.ToString()
                ))
                .ToList();
        }

        // ---------------- DRIVE APPLICATIONS ----------------
        public async Task<List<ApplicationResponse>> GetDriveApplicationsAsync(
            Guid driveId,
            CancellationToken cancellationToken)
        {
            if (driveId == Guid.Empty)
                throw new ValidationException("Invalid drive ID");

            var applications = await _repository.GetByDriveIdAsync(driveId, cancellationToken);

            return applications
                .Select(a => new ApplicationResponse(
                    a.Id,
                    a.DriveId,
                    a.StudentUserId,
                    a.AppliedAt,
                    a.Status.ToString()
                ))
                .ToList();
        }
    }
}