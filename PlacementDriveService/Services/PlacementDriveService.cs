using Microsoft.EntityFrameworkCore;
using PlacementDriveService.Constants;
using PlacementDriveService.DTOs;
using PlacementDriveService.Entities;
using PlacementDriveService.Repositries.Interfaces;
using PlacementDriveService.Services.Interfaces;

namespace PlacementDriveService.Services
{
    public class PlacementDriveService : IPlacementDriveService
    {
        private readonly IPlacementDriveRepository _repo;
        private readonly IKafkaClient _kafkaClient;

        public PlacementDriveService(IPlacementDriveRepository repo, IKafkaClient kafkaClient)
        {
            _repo = repo;
            _kafkaClient = kafkaClient;
        }

        public async Task<Guid> CreateDriveAsync(PlacementDriveCreateDto dto)
        {
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
                CreatedBy = "",
            };

            await _repo.AddAsync(drive);
            await _repo.SaveChangesAsync();
            await _kafkaClient.Publish(
                topic: "drive.events",
                key: drive.Id.ToString(),
                message: new
                {
                    EventId = drive.Id,
                    EventType = "DrivePublished",
                    UserId = drive.CreatedBy,
                });

            return drive.Id;
        }

        public async Task UpdateDriveAsync(Guid id, PlacementDriveUpdateDto dto)
        {
            var drive = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Placement drive not found");

            if (dto.Package.HasValue && dto.Package < 0)
                throw new ArgumentException("Package cannot be negative");

            drive.CompanyName = dto.CompanyName ?? drive.CompanyName;
            drive.JobRole = dto.JobRole ?? drive.JobRole;
            drive.Package = dto.Package ?? drive.Package;
            drive.Description = dto.Description ?? drive.Description;
            drive.AllowedBranches = dto.AllowedBranches ?? drive.AllowedBranches;
            drive.DriveDate = dto.DriveDate ?? drive.DriveDate;
            drive.ApplicationDeadline = dto.ApplicationDeadline ?? drive.ApplicationDeadline;
            drive.Status = dto.Status ?? drive.Status;

            await _repo.SaveChangesAsync();
        }

        public async Task DeleteDriveAsync(Guid id)
        {
            var drive = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Placement drive not found");

            await _repo.DeleteAsync(drive);
            await _repo.SaveChangesAsync();
        }

        public async Task<List<PlacementDriveResponseDto>> GetOpenDrivesAsync(int page, int pageSize)
        {
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
            var drive = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Placement drive not found");

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

        public async Task ApplyAsync(Guid driveId, Guid studentId)
        {
            if (await _repo.HasStudentApplied(driveId, studentId))
                throw new InvalidOperationException("Already applied");

            var application = new PlacementApplication
            {
                Id = Guid.NewGuid(),
                PlacementDriveId = driveId,
                StudentUserId = studentId,
                AppliedAt = DateTime.UtcNow,
                Status = "Applied"
            };

            await _repo.AddApplicationAsync(application);
            await _repo.SaveChangesAsync();
        }

        public async Task WithdrawAsync(Guid driveId, Guid studentId)
        {
            var application = await _repo.GetApplication(driveId, studentId)
                ?? throw new KeyNotFoundException("Application not found");

            await _repo.DeleteAsync(application.PlacementDrive);
            await _repo.SaveChangesAsync();
        }

        public async Task<List<PlacementApplicationResponseDto>> GetStudentApplications(Guid studentId)
        {
            return await _repo.GetStudentApplications(studentId)
                .Select(a => new PlacementApplicationResponseDto
                {
                    ApplicationId = a.Id,
                    PlacementDriveId = a.PlacementDriveId,
                    CompanyName = a.PlacementDrive.CompanyName,
                    JobRole = a.PlacementDrive.JobRole,
                    Package = a.PlacementDrive.Package,
                    Status = a.Status,
                    AppliedAt = a.AppliedAt
                })
                .ToListAsync();
        }
    }
}
