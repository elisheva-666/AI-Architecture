using Confluent.Kafka;
using System.Text.Json;

namespace ChineseAuction.Api.Services
{
    /// <summary>
    /// Wraps Confluent.Kafka ProducerBuilder.
    /// Registered as Singleton — one producer instance reused for all messages.
    /// </summary>
    public class KafkaProducerService : IKafkaProducer, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IConfiguration config, ILogger<KafkaProducerService> logger)
        {
            _logger = logger;

            var bootstrapServers = config["Kafka:BootstrapServers"]
                ?? throw new InvalidOperationException("Kafka:BootstrapServers is missing from configuration.");

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                // Guarantee at-least-once delivery
                Acks = Acks.All,
                // Retry on transient failures
                MessageSendMaxRetries = 3,
                RetryBackoffMs = 1000,
                // Prevent duplicate messages on retry
                EnableIdempotence = true
            };

            _producer = new ProducerBuilder<string, string>(producerConfig)
                .SetErrorHandler((_, e) =>
                    _logger.LogError("Kafka producer error: {Reason} (fatal={IsFatal})", e.Reason, e.IsFatal))
                .Build();

            _logger.LogInformation("Kafka producer initialised → {Servers}", bootstrapServers);
        }

        /// <summary>
        /// Serialises <paramref name="message"/> to JSON and publishes it to <paramref name="topic"/>.
        /// </summary>
        public async Task PublishAsync<T>(string topic, string key, T message)
        {
            var json = JsonSerializer.Serialize(message);

            try
            {
                var result = await _producer.ProduceAsync(topic, new Message<string, string>
                {
                    Key   = key,
                    Value = json
                });

                _logger.LogInformation(
                    "Kafka PUBLISHED → topic={Topic} partition={Partition} offset={Offset} key={Key}",
                    result.Topic, result.Partition.Value, result.Offset.Value, key);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex,
                    "Kafka PRODUCE FAILED → topic={Topic} key={Key} error={Error}",
                    topic, key, ex.Error.Reason);
                // Do not rethrow — Kafka failure should not break business flow
            }
        }

        public void Dispose()
        {
            // Flush pending messages before shutdown
            _producer.Flush(TimeSpan.FromSeconds(5));
            _producer.Dispose();
        }
    }
}
