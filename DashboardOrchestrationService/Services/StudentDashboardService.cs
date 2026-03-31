// Services/StudentDashboardService.cs
using DashboardOrchestrationService.Clients.Interfaces;
using DashboardOrchestrationService.DTOs;

namespace DashboardOrchestrationService.Services
{
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

        public async Task<StudentDashboardDto> GetStudentDashboardAsync(Guid studentId)
        {
            // Critical call (student profile) - fail fast if this fails, let it bubble up to middleware for standardized error handling
            var student = await _studentClient.GetStudentProfileById(studentId);

            var dashboard = new StudentDashboardDto
            {
                StudentId = student.Id,
                Name = student.Name,
                Email = student.Email,
                ProfileProgress = student.ProfileProgress,
                IsPlaced = student.IsPlaced,
                Stats = new DashboardStatsDto(),
                Errors = new List<string>(),
                ProfileCompletion = new ProfileCompletionDto
                {
                    IsAcademicInfoComplete = student.IsAcademicInfoComplete,
                    IsSkillsComplete = student.IsSkillsComplete,
                    IsContactComplete = student.IsContactComplete,
                    IsResumeComplete = student.IsResumeComplete
                }
            };

            // Non-critical — fan out in parallel
            var applicationsTask = _applicationClient.GetApplicationsByUserId(studentId);
            var totalDrivesTask = _driveClient.GetTotalEligibleDrivesCount(studentId);

            await Task.WhenAll(applicationsTask, totalDrivesTask);

            // Applications + drive enrichment
            try
            {
                var applications = await applicationsTask;

                dashboard.Stats.Applied = applications.Count;
                dashboard.Stats.Shortlisted = applications.Count(a => a.Status == "Shortlisted");
                dashboard.Stats.Selected = applications.Count(a => a.Status == "Selected");

                // Batch fetch drive details for the applications
                var driveIds = applications.Select(a => a.DriveId).Distinct().ToList();
                var drives = await _driveClient.GetDrivesByIds(driveIds);
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
                _logger.LogError(ex, "Failed to load applications for student {StudentId}", studentId);
                dashboard.Errors.Add("applications_unavailable");
            }

            // Total eligible drives count
            try
            {
                dashboard.Stats.TotalDrives = await totalDrivesTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load drive stats for student {StudentId}", studentId);
                dashboard.Errors.Add("drive_stats_unavailable");
            }

            return dashboard;
        }
    }
}