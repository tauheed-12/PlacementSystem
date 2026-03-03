using Confluent.Kafka;
using PlacementDriveService.Services.Interfaces;
using System.Text.Json;

public class KafkaClient : IKafkaClient
{
    private readonly IProducer<string, string> _producer;
    private readonly ConsumerConfig _consumerConfig;

    public KafkaClient(IConfiguration config)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"]
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();

        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = "default-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
    }

    //Publish
    public async Task Publish<T>(string topic, string key, T message)
    {
        var json = JsonSerializer.Serialize(message);

        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = key,
            Value = json
        });
    }

    //Consume
    public async IAsyncEnumerable<T> Consume<T>(
        string topic,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();

        consumer.Subscribe(topic);

        while (!cancellationToken.IsCancellationRequested)
        {
            var result = consumer.Consume(cancellationToken);

            var message = JsonSerializer.Deserialize<T>(result.Message.Value);
            yield return message!;
        }
    }
}