using PlacementDriveService.Enums;

namespace PlacementDriveService.DTOs
{
    public record DriveCreateRequest(string CompanyName, string JobRole, decimal Package, string Description, List<string> AllowedBranches, DateTime DriveDate, DateTime ApplicationDeadline);
    public record DriveResponse(Guid Id, string CompanyName, string JobRole, decimal Package, string Description, List<string> AllowedBranches, DateTime DriveDate, DateTime ApplicationDeadline, DriveStatus Status);
    public record DriveUpdateRequest(string? CompanyName, string? JobRole, decimal? Package, string? Description, List<string>? AllowedBranches, DateTime? DriveDate, DateTime? ApplicationDeadline, DriveStatus? Status);
}