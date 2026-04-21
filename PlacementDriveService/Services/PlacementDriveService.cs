using Microsoft.EntityFrameworkCore;
using PlacementDriveService.DTOs;
using PlacementDriveService.Entities;
using PlacementDriveService.Enums;
using PlacementDriveService.Events;
using Common.Contracts.Web;
using PlacementDriveService.Repositries.Interfaces;
using PlacementDriveService.Services.Interfaces;
using System.Text.Json;

namespace PlacementDriveService.Services
{
    public class PlacementDriveService : IPlacementDriveService
    {
        private readonly IPlacementDriveRepository _repo;
        private readonly ILogger<PlacementDriveService> _logger;

        public PlacementDriveService(IPlacementDriveRepository repo, ILogger<PlacementDriveService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        // ---------------- CREATE ----------------
        public async Task<Guid> CreateDriveAsync(DriveCreateRequest request, Guid UserId, CancellationToken ct)
        {
            _logger.LogInformation("Creating placement drive for company {Company}", request.CompanyName);

            var drive = new PlacementDrive
            {
                Id = Guid.NewGuid(),
                CompanyName = request.CompanyName.Trim(),
                JobRole = request.JobRole.Trim(),
                Package = request.Package,
                Description = request.Description?.Trim() ?? string.Empty,
                AllowedBranches = request.AllowedBranches,
                DriveDate = request.DriveDate,
                ApplicationDeadline = request.ApplicationDeadline,
                Status = DriveStatus.Scheduled,
            };

            await _repo.AddAsync(drive, ct);
            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("Placement drive created successfully with Id {DriveId}", drive.Id);

            var message = new DriveCreatedEvent
            {
                EventId = drive.Id,
                EventType = "DriverCreated",
                AudienceType = "Broadcast",
                Data = new Dictionary<string, string>
                    {
                        { "Company" , drive.CompanyName}
                    }
            };

            var newOutboxMsg = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = nameof(DriveCreatedEvent),
                Payload = JsonSerializer.Serialize(message),
                CreatedAt = DateTime.UtcNow,
                Key = UserId.ToString(),
            };

            await _repo.AddOutboxMessageAsync(newOutboxMsg, ct);
            _logger.LogInformation("User registration event add for user {CompanyName}", request.CompanyName);

            await _repo.SaveChangesAsync(ct);
            _logger.LogInformation("Kafka event published for drive {DriveId}", drive.Id);

            return drive.Id;
        }

        // ---------------- UPDATE ----------------
        public async Task UpdateDriveAsync(Guid id, DriveUpdateRequest request, CancellationToken ct)
        {
            if (id == Guid.Empty)
                throw new ValidationException("Invalid drive ID");

            _logger.LogInformation("Updating placement drive {DriveId}", id);

            var drive = await _repo.GetByIdAsync(id, ct);

            if (drive == null)
            {
                _logger.LogWarning("Drive not found: {DriveId}", id);
                throw new NotFoundException("Placement drive not found");
            }

            drive.CompanyName = request.CompanyName?.Trim() ?? drive.CompanyName;
            drive.JobRole = request.JobRole?.Trim() ?? drive.JobRole;
            drive.Package = request.Package ?? drive.Package;
            drive.Description = request.Description?.Trim() ?? drive.Description;
            drive.AllowedBranches = request.AllowedBranches ?? drive.AllowedBranches;
            drive.DriveDate = request.DriveDate ?? drive.DriveDate;
            drive.ApplicationDeadline = request.ApplicationDeadline ?? drive.ApplicationDeadline;
            drive.Status = request.Status ?? drive.Status;

            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("Drive updated successfully {DriveId}", id);
        }

        // ---------------- DELETE ----------------
        public async Task DeleteDriveAsync(Guid id, CancellationToken ct)
        {
            if (id == Guid.Empty)
                throw new ValidationException("Invalid drive ID");

            _logger.LogInformation("Deleting placement drive {DriveId}", id);

            var drive = await _repo.GetByIdAsync(id, ct);

            if (drive == null)
            {
                _logger.LogWarning("Drive not found for deletion {DriveId}", id);
                throw new NotFoundException("Placement drive not found");
            }

            await _repo.DeleteAsync(drive, ct);
            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("Drive deleted successfully {DriveId}", id);
        }

        // ---------------- GET OPEN DRIVES ----------------
        public async Task<List<DriveResponse>> GetOpenDrivesAsync(int page, int pageSize, CancellationToken ct)
        {
            if (page <= 0)
                throw new ValidationException("Page must be greater than 0");

            if (pageSize <= 0 || pageSize > 100)
                throw new ValidationException("Page size must be between 1 and 100");

            _logger.LogInformation("Fetching open drives page {Page}", page);

            return await _repo.GetOpenDrives(ct)
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DriveResponse
                (
                    d.Id,
                    d.CompanyName,
                    d.JobRole,
                    d.Package,
                    d.Description,
                    d.AllowedBranches,
                    d.DriveDate,
                    d.ApplicationDeadline,
                    d.Status
                ))
                .ToListAsync(ct);
        }

        // ---------------- GET BY ID ----------------
        public async Task<DriveResponse> GetDriveByIdAsync(Guid id, CancellationToken ct)
        {
            if (id == Guid.Empty)
                throw new ValidationException("Invalid drive ID");

            _logger.LogInformation("Fetching drive {DriveId}", id);

            var drive = await _repo.GetByIdAsync(id, ct);

            if (drive == null)
            {
                _logger.LogWarning("Drive not found {DriveId}", id);
                throw new NotFoundException("Placement drive not found");
            }

            return new DriveResponse
            (
                drive.Id,
                drive.CompanyName,
                drive.JobRole,
                drive.Package,
                drive.Description,
                drive.AllowedBranches,
                drive.DriveDate,
                drive.ApplicationDeadline,
                drive.Status
            );
        }

        // ---------------- BULK FETCH ----------------
        public async Task<List<DriveResponse>> GetDrivesBulkAsync(List<Guid> driveIds, CancellationToken ct)
        {
            if (driveIds == null || driveIds.Count == 0)
            {
                _logger.LogWarning("No drive IDs provided for bulk fetch");
                return new List<DriveResponse>();
            }

            var distinctIds = driveIds.Distinct().Take(100).ToList();
            var drives = new List<DriveResponse>();

            foreach (var id in distinctIds)
            {
                if (id == Guid.Empty) continue;

                var drive = await _repo.GetByIdAsync(id, ct);

                if (drive != null)
                {
                    drives.Add(new DriveResponse
                    (
                        drive.Id,
                        drive.CompanyName,
                        drive.JobRole,
                        drive.Package,
                        drive.Description,
                        drive.AllowedBranches,
                        drive.DriveDate,
                        drive.ApplicationDeadline,
                        drive.Status
                    ));
                }
            }

            _logger.LogInformation("Bulk fetched {Count} drives", drives.Count);
            return drives;
        }
    }
}