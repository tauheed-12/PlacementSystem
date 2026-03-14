using Confluent.Kafka;
using NotificationsService.Clients.Interfaces;
using NotificationsService.Entites;
using NotificationsService.Entities;
using System.Text.Json;

namespace NotificationsService.Clients
{
    public class KafkaClient : IKafkaClient, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ConsumerConfig _consumerConfig;

        public KafkaClient()
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = "localhost:9092",
                Acks = Acks.All
            };

            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "notification-service",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        // PRODUCE MESSAGE
        public async Task Publish<T>(string topic, string key, T message)
        {
            var payload = JsonSerializer.Serialize(message);

            await _producer.ProduceAsync(
                topic,
                new Message<string, string>
                {
                    Key = key,
                    Value = payload
                });
        }

        public async Task Publish(string topic, NotificationDelivery message)
        {
            var payload = JsonSerializer.Serialize(message);
            await _producer.ProduceAsync(
                topic,
                new Message<string, string>
                {
                    Value = payload
                });
        }

        // CONSUME MESSAGE
        public async IAsyncEnumerable<(T Message, IConsumer<string, string> Consumer, ConsumeResult<string, string> Result)>
        Consume<T>(
            string topic,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken token)
        {
            var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
            consumer.Subscribe(topic);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    ConsumeResult<string, string>? result;

                    try
                    {
                        result = consumer.Consume(token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    if (result?.Message?.Value == null)
                        continue;

                    var message = JsonSerializer.Deserialize<T>(result.Message.Value)!;

                    yield return (message, consumer, result);

                    await Task.Yield();
                }
            }
            finally
            {
                try { consumer.Close(); } catch { }
                consumer.Dispose();
            }
        }

        // MANUAL COMMIT
        public void Commit(IConsumer<string, string> consumer, ConsumeResult<string, string> result)
        {
            consumer.Commit(result);
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}