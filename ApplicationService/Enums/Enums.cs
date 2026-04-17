namespace ApplicationService.Enums
{
    public enum ApplicationStatus
    {
        Applied = 1,
        Shortlisted = 2,
        Rejected = 3,
        Selected = 4,
    }

    public static class Roles
    {
        public const string Student = "Student";
        public const string Admin = "Admin";
        public const string Recruiter = "Recruiter";
        public const string PlacementCoordinator = "PlacementCoordinator";
        public const string TPO = "TPO";
    }
}
