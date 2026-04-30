using AuthService.Services.Interfaces;
using Confluent.Kafka;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;

namespace AuthService.Services
{
    public class KafkaService : IKafkaService, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ConsumerConfig _consumerConfig;
        private readonly ILogger<KafkaService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        };

        public KafkaService(ILogger<KafkaService> logger, IConfiguration config)
        {
            _logger = logger;

            var bootstrapServers = config["Kafka:BootstrapServers"]
                ?? throw new InvalidOperationException("Kafka:BootstrapServers is not configured.");

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All
            };

            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = "auth-service",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        public async Task Publish<T>(string topic, string key, T message)
        {
            var payload = JsonSerializer.Serialize(message, _jsonOptions);
            _logger.LogInformation("Publishing to {Topic} key={Key}: {Payload}", topic, key, payload);

            await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = key,
                Value = payload
            });

            _logger.LogInformation("Published to {Topic} key={Key}", topic, key);
        }

        public async Task PublishRaw(string topic, string key, string payload)
        {
            _logger.LogInformation("Publishing raw to {Topic} key={Key}: {Payload}", topic, key, payload);

            await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = key,
                Value = payload
            });

            _logger.LogInformation("Published raw to {Topic} key={Key}", topic, key);
        }

        public async IAsyncEnumerable<(T Message, IConsumer<string, string> Consumer, ConsumeResult<string, string> Result)>
        Consume<T>(string topic, [EnumeratorCancellation] CancellationToken token)
        {
            var consumerConfig = new ConsumerConfig(_consumerConfig);
            var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            consumer.Subscribe(topic);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    ConsumeResult<string, string>? result = null;

                    try
                    {
                        result = await Task.Run(() => consumer.Consume(token), token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    if (result?.Message?.Value == null)
                        continue;

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
        {
            consumer.Commit(result);
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}