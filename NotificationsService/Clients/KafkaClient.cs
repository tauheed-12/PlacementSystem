using Confluent.Kafka;
using NotificationsService.Clients.Interfaces;
using NotificationsService.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;

namespace NotificationsService.Clients
{
    public class KafkaClient : IKafkaClient, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ConsumerConfig _consumerConfig;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        };

        public KafkaClient(IConfiguration config)
        {
            var bootstrapServers = config["Kafka:BootstrapServers"]
                ?? throw new InvalidOperationException("Kafka:BootstrapServers is not configured.");

            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _producer = new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All
            }).Build();
        }

        public async Task Publish<T>(string topic, string key, T message)
        {
            var payload = JsonSerializer.Serialize(message, _jsonOptions);
            await _producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = payload });
        }

        public async Task PublishRaw(string topic, string key, string message)
        {
            await _producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = message });
        }

        public async IAsyncEnumerable<(T Message, IConsumer<string, string> Consumer, ConsumeResult<string, string> Result)>
        Consume<T>(string topic, string groupId, [EnumeratorCancellation] CancellationToken token)
        {
            var config = new ConsumerConfig(_consumerConfig) { GroupId = groupId };
            var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(topic);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    ConsumeResult<string, string>? result = null;
                    try { result = await Task.Run(() => consumer.Consume(token), token); }
                    catch (OperationCanceledException) { break; }

                    if (result?.Message?.Value == null) continue;

                    var message = JsonSerializer.Deserialize<T>(result.Message.Value, _jsonOptions)!;
                    yield return (message, consumer, result);
                }
            }
            finally
            {
                try { consumer.Close(); } catch { }
                consumer.Dispose();
            }
        }

        public void Commit(IConsumer<string, string> consumer, ConsumeResult<string, string> result)
            => consumer.Commit(result);

        public void Dispose() => _producer?.Dispose();
    }
}