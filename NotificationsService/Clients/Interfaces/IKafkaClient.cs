using Confluent.Kafka;

namespace NotificationsService.Clients.Interfaces
{
    public interface IKafkaClient
    {
        IAsyncEnumerable<(T Message, IConsumer<string, string> Consumer, ConsumeResult<string, string> Result)> Consume<T>(string topic, string groupId, CancellationToken token);
        Task Publish<T>(string topic, string key, T message);
        Task PublishRaw(string topic, string key, string message);
        void Commit(IConsumer<string, string> consumer, ConsumeResult<string, string> result);
    }
}