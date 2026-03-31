namespace PlacementDriveService.Events
{
    public class DriveCreatedEvent
    {
        public Guid EventId { get; set; }
        public string EventType { get; set; } = "DriverCreated";
        public string AudienceType { get; set; } = "Broadcast";
        public Dictionary<string, string>? Data { get; set; }
    }
}
