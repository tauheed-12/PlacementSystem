namespace PlacementDriveService.Entities
{
    public class PlacementApplication
    {
        public Guid Id { get; set; }
        public Guid PlacementDriveId { get; set; }
        public Guid StudentUserId { get; set; }
        public string Status { get; set; } = "Applied";
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public PlacementDrive PlacementDrive { get; set; } = null!;
    }
}
