using Confluent.Kafka;
using System.Text.Json;

namespace NotificationService.Infrastructure.Kafka
{
    public class KafkaClient : IKafkaClient
    {
        private readonly ProducerConfig _producerConfig;
        private readonly ConsumerConfig _consumerConfig;

        public KafkaClient()
        {
            _producerConfig = new ProducerConfig
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
        }

        public async Task Publish<T>(string topic, string key, T message)
        {
            using var producer = new ProducerBuilder<string, string>(_producerConfig).Build();
            var payload = JsonSerializer.Serialize(message);

            await producer.ProduceAsync(
                topic,
                new Message<string, string>
                {
                    Key = key,
                    Value = payload
                }
            );
        }

        public async IAsyncEnumerable<T> Consume<T>(string topic)
        {
            using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
            consumer.Subscribe(topic);
            while(true)
            {
                var result = consumer.Consume();
                var message = JsonSerializer.Deserialize<T>(result.Message.Value)!;

                yield return message;

                consumer.Commit(result);
                await Task.Yield();
            }
        }
    }
}
