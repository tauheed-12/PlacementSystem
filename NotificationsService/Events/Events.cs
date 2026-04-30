namespace NotificationsService.Events;

public class EmailDeliveryEvent
{
    public Guid DeliveryId { get; set; }
    public Guid IntentId { get; set; }
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
}

public class InAppDeliveryEvent
{
    public Guid DeliveryId { get; set; }
    public Guid IntentId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } =  null!;
}