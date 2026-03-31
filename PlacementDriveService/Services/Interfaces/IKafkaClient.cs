using Confluent.Kafka;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PlacementDriveService.Services.Interfaces
{
    public interface IKafkaClient
    {
        IAsyncEnumerable<(T Message, IConsumer<string, string> Consumer, ConsumeResult<string, string> Result)> Consume<T>(string topic, CancellationToken token);
        Task Publish<T>(string topic, string key, T message);
        void Commit(IConsumer<string, string> consumer, ConsumeResult<string, string> result);
    }
}
