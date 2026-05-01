using ApplicationService.Enums;

namespace ApplicationService.DTO
{
    public static class Dtos
    {
        // ---------------- REQUEST ----------------

        public record CreateApplicationRequest(
            Guid DriveId,
            Guid StudentId
        );

        public record ApplyRequest(
            Guid DriveId
        );
        // ---------------- RESPONSE ----------------

        public record ApplicationResponse(
            Guid ApplicationId,
            Guid DriveId,
            Guid StudentId,
            DateTime AppliedAt,
            ApplicationStatus Status
        );

        public record UserApplicationSummary(
            Guid ApplicationId,
            string CompanyName,
            ApplicationStatus Status,
            DateTime AppliedAt,
            DateTime DriveDate
        );

        public record PlacementDriveDetails(
            Guid Id,
            string CompanyName,
            string JobRole,
            decimal Package,
            string Description,
            List<string> AllowedBranches,
            DateTime DriveDate,
            DateTime ApplicationDeadline,
            string Status
        );

        public record StudentApplication(
            Guid Id,
            Guid DriveId,
            DateTime AppliedAt,
            ApplicationStatus Status
        );

        public record ApiResponse<T>(bool Success, string? Message, T? Data);
        public record ApiErrorResponse(bool Success, string Message, IEnumerable<string>? Errors = null);
    }
}
