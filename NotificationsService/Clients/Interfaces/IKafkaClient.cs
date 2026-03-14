using Confluent.Kafka;
using NotificationsService.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationsService.Clients.Interfaces
{
    public interface IKafkaClient
    {
        IAsyncEnumerable<(T Message, IConsumer<string, string> Consumer, ConsumeResult<string, string> Result)> Consume<T>(string topic, CancellationToken token);
        Task Publish<T>(string topic, string key, T message);
        Task Publish(string topic, NotificationDelivery delivery);
        void Commit(IConsumer<string, string> consumer, ConsumeResult<string, string> result);
    }
}