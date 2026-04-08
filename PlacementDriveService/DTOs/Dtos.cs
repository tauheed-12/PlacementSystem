using PlacementDriveService.Enums;
using System.ComponentModel.DataAnnotations;

namespace PlacementDriveService.DTOs
{
    public record DriveCreateRequest(
        [Required]
        [MaxLength(200, ErrorMessage = "Company name cannot exceed 200 characters.")]
        string CompanyName,
        [Required]
        [MaxLength(100, ErrorMessage = "Job role cannot exceed 100 characters.")]
        string JobRole,
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Package must be a positive value.")]
        decimal Package,
        [Required]
        string Description,
        [Required]
        List<string> AllowedBranches,
        [Required]
        DateTime DriveDate,
        [Required]
        DateTime ApplicationDeadline
    );

    public record DriveResponse(
        Guid Id,
        string CompanyName,
        string JobRole,
        decimal Package,
        string Description,
        List<string> AllowedBranches,
        DateTime DriveDate,
        DateTime ApplicationDeadline,
        DriveStatus Status
    );

    public record DriveUpdateRequest(
        [MaxLength(200, ErrorMessage = "Company name cannot exceed 200 characters.")]
        string? CompanyName,
        [MaxLength(100, ErrorMessage = "Job role cannot exceed 100 characters.")]
        string? JobRole,
        [Range(0, double.MaxValue, ErrorMessage = "Package must be a positive value.")]
        decimal? Package,
        string? Description,
        List<string>? AllowedBranches,
        DateTime? DriveDate,
        DateTime? ApplicationDeadline, 
        DriveStatus? Status
    );
}