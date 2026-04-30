using NotificationsService.Enums;

namespace NotificationsService.Events;

public record NotificationEvent
{
    public Guid EventId { get; init; }
    public NotificationEventType EventType { get; init; }
    public NotificationAudience AudienceType { get; init; }
    public List<Guid>? TargetUserIds { get; init; }
    public Dictionary<string, string> Data { get; init; } = new();
}