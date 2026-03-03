namespace NotificationService.Infrastructure.Kafka
{
    public interface IKafkaClient
    {
        IAsyncEnumerable<T> Consume<T>(string topic);
        Task Publish<T>(string topic, string key, T message);
    }
}
