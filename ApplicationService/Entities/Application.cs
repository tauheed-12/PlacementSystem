using ApplicationService.Enums;
using ApplicationService.Middleware;

namespace ApplicationService.Entities
{
    public class Application
    {
        public Guid Id { get; private set; }
        public Guid StudentUserId { get; private set; }
        public Guid DriveId { get;private set; }
        public ApplicationStatus Status { get; private set; } = ApplicationStatus.Applied;
        public DateTime AppliedAt { get; private set; } = DateTime.Now;

        public static Application Create(Guid studentId, Guid driveId)
        {
            if (studentId == Guid.Empty) throw new ValidationException("StudentId is Required");
            if (driveId == Guid.Empty) throw new ValidationException("DriveId is required");

            return new Application
            {
                Id = Guid.NewGuid(),
                StudentUserId = studentId,
                DriveId = driveId,
                Status = ApplicationStatus.Applied,
                AppliedAt = DateTime.Now,
            };
        }
    }
}
