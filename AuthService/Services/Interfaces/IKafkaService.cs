using Confluent.Kafka;

namespace AuthService.Services.Interfaces
{
    public interface IKafkaService
    {
        IAsyncEnumerable<(T Message, IConsumer<string, string> Consumer, ConsumeResult<string, string> Result)> Consume<T>(string topic, CancellationToken token);
        Task Publish<T>(string topic, string key, T message);
        Task PublishRaw(string topic, string key, string payload);
        void Commit(IConsumer<string, string> consumer, ConsumeResult<string, string> result);
    }
}
