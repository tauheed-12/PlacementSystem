using FluentValidation;
using PlacementDriveService.DTOs;

namespace PlacementDriveService.Validators
{
    public class DriveCreateRequestValidator : AbstractValidator<DriveCreateRequest>
    {
        public DriveCreateRequestValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request cannot be null");

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required")
                .MaximumLength(100).WithMessage("Company name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.Package)
                .GreaterThan(0)
                .WithMessage("Package must be greater than 0");

            RuleFor(x => x.DriveDate)
                .Must(d => d > DateTime.UtcNow)
                .WithMessage("Drive date must be in the future");

            RuleFor(x => x.ApplicationDeadline)
                .Must(d => d > DateTime.UtcNow)
                .WithMessage("Application deadline must be in the future");

            RuleFor(x => x)
                .Must(x => x.ApplicationDeadline <= x.DriveDate)
                .WithMessage("Application deadline cannot be after drive date");

            RuleFor(x => x.AllowedBranches)
                .NotNull().WithMessage("Allowed branches are required")
                .Must(x => x.Any(b => b == "CSE" || b == "ECE" || b == "ME" || b == "CE" || b == "EE"))
                .WithMessage("At least one branch must be allowed");
        }
    }

    public class DriveUpdateRequestValidator : AbstractValidator<DriveUpdateRequest>
    {
        public DriveUpdateRequestValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("Request cannot be null");

            RuleFor(x => x.CompanyName)
                .MaximumLength(100)
                .WithMessage("Company name cannot exceed 100 characters")
                .When(x => x.CompanyName != null);

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Description cannot exceed 1000 characters")
                .When(x => x.Description != null);

            RuleFor(x => x.Package)
                .GreaterThan(0)
                .WithMessage("Package must be greater than 0")
                .When(x => x.Package.HasValue);

            RuleFor(x => x.DriveDate)
                .Must(d => !d.HasValue || d.Value > DateTime.UtcNow)
                .WithMessage("Drive date must be in the future");

            RuleFor(x => x.ApplicationDeadline)
                .Must(d => !d.HasValue || d.Value > DateTime.UtcNow)
                .WithMessage("Application deadline must be in the future");

            RuleFor(x => x)
                .Must(x =>
                    !x.ApplicationDeadline.HasValue ||
                    !x.DriveDate.HasValue ||
                    x.ApplicationDeadline <= x.DriveDate)
                .WithMessage("Application deadline cannot be after drive date");

            RuleFor(x => x.AllowedBranches)
                .Must(x => x == null || x.Any())
                .WithMessage("At least one branch must be allowed when provided");
        }
    }
}