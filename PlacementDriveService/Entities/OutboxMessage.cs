namespace PlacementDriveService.Entities
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string EventType { get; set; } = default!;
        public string Payload { get; set; } = default!;
        public string Key { get; set; } = default!;
        public bool IsProcessed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
    }
}
