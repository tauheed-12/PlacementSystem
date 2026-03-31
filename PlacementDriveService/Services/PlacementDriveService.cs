using Microsoft.EntityFrameworkCore;
using PlacementDriveService.Constants;
using PlacementDriveService.DTOs;
using PlacementDriveService.Entities;
using PlacementDriveService.Events;
using PlacementDriveService.Exceptions;
using PlacementDriveService.Repositries.Interfaces;
using PlacementDriveService.Services.Interfaces;

namespace PlacementDriveService.Services
{
    public class PlacementDriveService : IPlacementDriveService
    {
        private readonly IPlacementDriveRepository _repo;
        private readonly IKafkaClient _kafkaClient;
        private readonly ILogger<PlacementDriveService> _logger;

        public PlacementDriveService(
            IPlacementDriveRepository repo,
            IKafkaClient kafkaClient,
            ILogger<PlacementDriveService> logger)
        {
            _repo = repo;
            _kafkaClient = kafkaClient;
            _logger = logger;
        }

        public async Task<Guid> CreateDriveAsync(PlacementDriveCreateDto dto, Guid UserId)
        {
            _logger.LogInformation("Creating placement drive for company {Company}", dto.CompanyName);

            var drive = new PlacementDrive
            {
                Id = Guid.NewGuid(),
                CompanyName = dto.CompanyName,
                JobRole = dto.JobRole,
                Package = dto.Package,
                Description = dto.Description,
                AllowedBranches = dto.AllowedBranches,
                DriveDate = dto.DriveDate,
                ApplicationDeadline = dto.ApplicationDeadline,
                Status = DriveStatus.Scheduled,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = UserId
            };

            await _repo.AddAsync(drive);
            await _repo.SaveChangesAsync();

            _logger.LogInformation("Placement drive created successfully with Id {DriveId}", drive.Id);

            await _kafkaClient.Publish(
                topic: "notifications.events",
                key: drive.Id.ToString(),
                message: new DriveCreatedEvent
                {
                    EventId = drive.Id,
                    EventType = "DriverCreated",
                    AudienceType = "Broadcast",
                    Data = new Dictionary<string, string>
                    {
                        { "Company" , drive.CompanyName}
                    }
                });

            _logger.LogInformation("Kafka event published for drive {DriveId}", drive.Id);

            return drive.Id;
        }

        public async Task UpdateDriveAsync(Guid id, PlacementDriveUpdateDto dto)
        {
            _logger.LogInformation("Updating placement drive {DriveId}", id);

            var drive = await _repo.GetByIdAsync(id);

            if (drive == null)
            {
                _logger.LogWarning("Drive not found: {DriveId}", id);
                throw new NotFoundException("Placement drive not found");
            }

            if (dto.Package.HasValue && dto.Package < 0)
            {
                _logger.LogWarning("Invalid package value for drive {DriveId}", id);
                throw new ValidationException("Package cannot be negative");
            }

            drive.CompanyName = dto.CompanyName ?? drive.CompanyName;
            drive.JobRole = dto.JobRole ?? drive.JobRole;
            drive.Package = dto.Package ?? drive.Package;
            drive.Description = dto.Description ?? drive.Description;
            drive.AllowedBranches = dto.AllowedBranches ?? drive.AllowedBranches;
            drive.DriveDate = dto.DriveDate ?? drive.DriveDate;
            drive.ApplicationDeadline = dto.ApplicationDeadline ?? drive.ApplicationDeadline;
            drive.Status = dto.Status ?? drive.Status;

            await _repo.SaveChangesAsync();

            _logger.LogInformation("Drive updated successfully {DriveId}", id);
        }

        public async Task DeleteDriveAsync(Guid id)
        {
            _logger.LogInformation("Deleting placement drive {DriveId}", id);

            var drive = await _repo.GetByIdAsync(id);

            if (drive == null)
            {
                _logger.LogWarning("Drive not found for deletion {DriveId}", id);
                throw new NotFoundException("Placement drive not found");
            }

            await _repo.DeleteAsync(drive);
            await _repo.SaveChangesAsync();

            _logger.LogInformation("Drive deleted successfully {DriveId}", id);
        }

        public async Task<List<PlacementDriveResponseDto>> GetOpenDrivesAsync(int page, int pageSize)
        {
            _logger.LogInformation("Fetching open drives page {Page}", page);

            return await _repo.GetOpenDrives()
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new PlacementDriveResponseDto
                {
                    Id = d.Id,
                    CompanyName = d.CompanyName,
                    JobRole = d.JobRole,
                    Package = d.Package,
                    AllowedBranches = d.AllowedBranches,
                    DriveDate = d.DriveDate,
                    ApplicationDeadline = d.ApplicationDeadline,
                    Status = d.Status
                })
                .ToListAsync();
        }

        public async Task<PlacementDriveResponseDto> GetDriveByIdAsync(Guid id)
        {
            _logger.LogInformation("Fetching drive {DriveId}", id);

            if(id == Guid.Empty)
            {
                _logger.LogWarning("Invalid drive ID provided: {DriveId}", id);
                throw new ValidationException("Invalid drive ID");
            }

            var drive = await _repo.GetByIdAsync(id);

            if (drive == null)
            {
                _logger.LogWarning("Drive not found {DriveId}", id);
                throw new NotFoundException("Placement drive not found");
            }

            return new PlacementDriveResponseDto
            {
                Id = drive.Id,
                CompanyName = drive.CompanyName,
                JobRole = drive.JobRole,
                Package = drive.Package,
                Description = drive.Description,
                AllowedBranches = drive.AllowedBranches,
                DriveDate = drive.DriveDate,
                ApplicationDeadline = drive.ApplicationDeadline,
                Status = drive.Status
            };
        }

        public async Task<List<PlacementDriveResponseDto>> GetDrivesBulkAsync(List<Guid> driveIds)
        {
            if (driveIds == null || driveIds.Count == 0)
            {
                _logger.LogWarning("No drive IDs provided for bulk fetch");
                return new List<PlacementDriveResponseDto>();
            }

            var distinctIds = driveIds.Distinct().Take(100).ToList();
            var drives = new List<PlacementDriveResponseDto>();

            foreach (var id in distinctIds)
            {
                var drive = await _repo.GetByIdAsync(id);
                if (drive != null)
                {
                    drives.Add(new PlacementDriveResponseDto
                    {
                        Id = drive.Id,
                        CompanyName = drive.CompanyName,
                        JobRole = drive.JobRole,
                        Package = drive.Package,
                        Description = drive.Description,
                        AllowedBranches = drive.AllowedBranches,
                        DriveDate = drive.DriveDate,
                        ApplicationDeadline = drive.ApplicationDeadline,
                        Status = drive.Status
                    });
                }
            }

            _logger.LogInformation("Bulk fetched {Count} drives", drives.Count);
            return drives;
        }
    }
}