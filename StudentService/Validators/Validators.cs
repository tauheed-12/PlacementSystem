using FluentValidation;
using static StudentService.DTOs.Dtos;

namespace StudentService.Validators
{
    public class UpdateStudentProfileValidator : AbstractValidator<UpdateStudentProfileRequest>
    {
        public UpdateStudentProfileValidator()
        {
            RuleFor(x => x.Year)
                .InclusiveBetween(1, 4)
                .When(x => x.Year.HasValue)
                .WithMessage("Year must be between 1 and 4");

            RuleFor(x => x.CGPA)
                .InclusiveBetween(0, 10)
                .When(x => x.CGPA.HasValue)
                .WithMessage("CGPA must be between 0 and 10");

            RuleFor(x => x.FullName)
                .NotEmpty()
                .When(x => x.FullName != null)
                .WithMessage("Full name cannot be empty");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .When(x => x.PhoneNumber != null)
                .WithMessage("Phone number cannot be empty");

            RuleForEach(x => x.Skills)
                .NotEmpty().WithMessage("Skill cannot be empty")
                .When(x => x.Skills != null);
        }
    }

    public class BulkUserIdsValidator : AbstractValidator<List<Guid>>
    {
        public BulkUserIdsValidator()
        {
            RuleFor(x => x)
                .NotNull().WithMessage("UserIds list cannot be null");

            RuleFor(x => x.Count)
                .LessThanOrEqualTo(100)
                .WithMessage("Maximum 100 user IDs allowed");

            RuleForEach(x => x)
                .NotEmpty().WithMessage("Invalid user ID");
        }
    }

    public class CreateStudentProfileValidator : AbstractValidator<CreateStudentProfileRequest>
    {
        public CreateStudentProfileValidator()
        {
            RuleFor(x => x.RollNo)
                .NotEmpty().WithMessage("Roll number is required");

            RuleFor(x => x.EnrollmentNo)
                .NotEmpty().WithMessage("Enrollment number is required");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required");

            RuleFor(x => x.Course)
                .NotEmpty().WithMessage("Course is required");

            RuleFor(x => x.Branch)
                .NotEmpty().WithMessage("Branch is required");

            RuleFor(x => x.Year)
                .InclusiveBetween(1, 4)
                .WithMessage("Year must be between 1 and 4");

            RuleFor(x => x.CGPA)
                .InclusiveBetween(0, 10)
                .WithMessage("CGPA must be between 0 and 10");

            RuleForEach(x => x.Skills)
                .NotEmpty().WithMessage("Skill cannot be empty")
                .When(x => x.Skills != null);
        }
    }

    public class AddSkillValidator : AbstractValidator<AddSkillRequest>
    {
        public AddSkillValidator()
        {
            RuleFor(x => x.SkillName)
                .NotEmpty().WithMessage("Skill name is required")
                .MaximumLength(100).WithMessage("Skill name cannot exceed 100 characters");
        }
    }
}
