namespace PlacementDriveService.Services.Interfaces
{
    public interface IKafkaClient
    {
        Task Publish<T>(string topic, string key, T message);
        IAsyncEnumerable<T> Consume<T>(string topic, CancellationToken cancellationToken);

    }
}
