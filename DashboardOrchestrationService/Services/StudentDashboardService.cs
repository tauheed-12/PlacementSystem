using DashboardOrchestrationService.Clients.Interfaces;
using DashboardOrchestrationService.DTOs;

namespace DashboardOrchestrationService.Services;

public class StudentDashboardService : IStudentDashboardService
{
    private readonly IStudentServiceClient _studentClient;
    private readonly IApplicationServiceClient _applicationClient;
    private readonly IPlacementDriveServiceClient _driveClient;
    private readonly ILogger<StudentDashboardService> _logger;

    public StudentDashboardService(
        IStudentServiceClient studentClient,
        IApplicationServiceClient applicationClient,
        IPlacementDriveServiceClient driveClient,
        ILogger<StudentDashboardService> logger)
    {
        _studentClient = studentClient;
        _applicationClient = applicationClient;
        _driveClient = driveClient;
        _logger = logger;
    }

    public async Task<StudentDashboardDto> GetStudentDashboardAsync(Guid studentId, CancellationToken ct)
    {
        var student = await _studentClient.GetStudentProfileAsync(studentId);

        var dashboard = new StudentDashboardDto
        {
            StudentId = student.StudentId,
            Name = student.Name,
            Email = student.Email,
            ProfileProgress = student.ProfileProgress,
            IsPlaced = student.IsPlaced,
            Errors = [],
            Stats = new DashboardStatsDto(),
            ProfileCompletion = new ProfileCompletionDto
            {
                IsAcademicInfoComplete = student.IsAcademicInfoComplete,
                IsSkillsComplete = student.IsSkillsComplete,
                IsContactComplete = student.IsContactComplete,
                IsResumeComplete = student.IsResumeComplete
            }
        };

        var applicationsTask = _applicationClient.GetApplicationsAsync(studentId);
        var drivesCountTask = _driveClient.GetEligibleDrivesCountAsync(studentId);

        await Task.WhenAll(applicationsTask, drivesCountTask);

        // Applications
        try
        {
            var applications = await applicationsTask;

            dashboard.Stats.Applied = applications.Count;
            dashboard.Stats.Shortlisted = applications.Count(a => a.Status == "Shortlisted");
            dashboard.Stats.Selected = applications.Count(a => a.Status == "Selected");

            var driveIds = applications.Select(a => a.DriveId).Distinct().ToList();
            var drives = await _driveClient.GetDrivesByIdsAsync(driveIds);
            var driveLookup = drives.ToDictionary(d => d.Id);

            dashboard.RecentApplications = applications
                .Where(a => driveLookup.ContainsKey(a.DriveId))
                .OrderByDescending(a => a.AppliedOn)
                .Take(10)
                .Select(a => new RecentApplicationDto
                {
                    ApplicationId = a.Id,
                    CompanyName = driveLookup[a.DriveId].CompanyName,
                    JobRole = driveLookup[a.DriveId].JobRole,
                    DriveDate = driveLookup[a.DriveId].DriveDate,
                    AppliedOn = a.AppliedOn,
                    Status = a.Status
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Applications load failed for {StudentId}", studentId);
            dashboard.Errors.Add("applications_unavailable");
        }

        // Drives count
        try
        {
            dashboard.Stats.TotalDrives = await drivesCountTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Drive stats failed for {StudentId}", studentId);
            dashboard.Errors.Add("drive_stats_unavailable");
        }

        return dashboard;
    }
}