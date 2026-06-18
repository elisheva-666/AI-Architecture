using Confluent.Kafka;
using KafkaConsumer.Worker;
using System.Text.Json;

namespace KafkaConsumer.Worker
{
    /// <summary>
    /// Long-running background service that consumes both
    /// "order-confirmed" and "lottery-drawn" topics and logs every message.
    /// </summary>
    public class AuctionConsumerWorker : BackgroundService
    {
        private readonly ILogger<AuctionConsumerWorker> _logger;
        private readonly IConfiguration _config;

        public AuctionConsumerWorker(ILogger<AuctionConsumerWorker> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run the blocking Kafka poll loop on a dedicated thread so it
            // does not block the host startup.
            return Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
        }

        private void ConsumeLoop(CancellationToken ct)
        {
            var bootstrapServers = _config["Kafka:BootstrapServers"]
                ?? throw new InvalidOperationException("Kafka:BootstrapServers missing");

            var groupId = _config["Kafka:ConsumerGroupId"] ?? "auction-consumer-group";
            var orderTopic   = _config["Kafka:OrderConfirmedTopic"]  ?? "order-confirmed";
            var lotteryTopic = _config["Kafka:LotteryDrawnTopic"]    ?? "lottery-drawn";

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId          = groupId,
                AutoOffsetReset  = AutoOffsetReset.Earliest,   // read from beginning if no committed offset
                EnableAutoCommit = false                        // manual commit after processing
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .SetErrorHandler((_, e) =>
                    _logger.LogError("Kafka consumer error: {Reason} (fatal={IsFatal})", e.Reason, e.IsFatal))
                .SetPartitionsAssignedHandler((c, partitions) =>
                    _logger.LogInformation("Partitions assigned: {Partitions}",
                        string.Join(", ", partitions.Select(p => $"{p.Topic}[{p.Partition}]"))))
                .Build();

            consumer.Subscribe(new[] { orderTopic, lotteryTopic });

            _logger.LogInformation(
                "Kafka consumer started → group={Group} topics=[{Topics}] servers={Servers}",
                groupId, string.Join(", ", new[] { orderTopic, lotteryTopic }), bootstrapServers);

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    ConsumeResult<string, string>? result = null;
                    try
                    {
                        result = consumer.Consume(ct);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Consume error: {Reason}", ex.Error.Reason);
                        continue;
                    }

                    if (result?.Message is null) continue;

                    try
                    {
                        HandleMessage(result.Topic, result.Message.Key, result.Message.Value,
                            result.Partition.Value, result.Offset.Value);

                        // Commit offset only after successful handling
                        consumer.Commit(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Error handling message from topic={Topic} partition={Partition} offset={Offset}",
                            result.Topic, result.Partition.Value, result.Offset.Value);
                        // Do NOT commit — message will be re-delivered
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Kafka consumer shutting down gracefully.");
            }
            finally
            {
                consumer.Close(); // commits offsets and leaves the group cleanly
            }
        }

        private void HandleMessage(string topic, string key, string value, int partition, long offset)
        {
            _logger.LogInformation(
                "┌─ Kafka MESSAGE RECEIVED ────────────────────────────────────────");
            _logger.LogInformation(
                "│  Topic={Topic}  Partition={Partition}  Offset={Offset}  Key={Key}",
                topic, partition, offset, key);

            if (topic.EndsWith("order-confirmed", StringComparison.OrdinalIgnoreCase))
            {
                var ev = JsonSerializer.Deserialize<OrderConfirmedEvent>(value);
                if (ev is null) { _logger.LogWarning("│  Failed to deserialise OrderConfirmedEvent"); return; }

                _logger.LogInformation("│  ✅ ORDER CONFIRMED");
                _logger.LogInformation("│     OrderId     : {OrderId}", ev.OrderId);
                _logger.LogInformation("│     Customer    : {Name} <{Email}>", ev.UserName, ev.UserEmail);
                _logger.LogInformation("│     TotalAmount : ₪{Total:F2}", ev.TotalAmount);
                _logger.LogInformation("│     ConfirmedAt : {At:dd/MM/yyyy HH:mm:ss}", ev.ConfirmedAt);
                _logger.LogInformation("│     Items ({Count}):", ev.Items.Count);
                foreach (var item in ev.Items)
                {
                    _logger.LogInformation(
                        "│       • [{GiftId}] {GiftName}  x{Qty}  ₪{Unit:F2}/unit  → ₪{Line:F2}",
                        item.GiftId, item.GiftName, item.Quantity, item.UnitPrice, item.LineTotal);
                }
            }
            else if (topic.EndsWith("lottery-drawn", StringComparison.OrdinalIgnoreCase))
            {
                var ev = JsonSerializer.Deserialize<LotteryDrawnEvent>(value);
                if (ev is null) { _logger.LogWarning("│  Failed to deserialise LotteryDrawnEvent"); return; }

                _logger.LogInformation("│  🏆 LOTTERY DRAWN");
                _logger.LogInformation("│     GiftId      : {GiftId}", ev.GiftId);
                _logger.LogInformation("│     Gift        : {GiftName}", ev.GiftName);
                _logger.LogInformation("│     Winner      : {Name} <{Email}> (userId={Id})",
                    ev.WinnerName, ev.WinnerEmail, ev.WinnerUserId);
                _logger.LogInformation("│     TotalTickets: {Total}", ev.TotalTickets);
                _logger.LogInformation("│     DrawnAt     : {At:dd/MM/yyyy HH:mm:ss}", ev.DrawnAt);
            }
            else
            {
                _logger.LogWarning("│  Unknown topic {Topic} — raw value: {Value}", topic, value);
            }

            _logger.LogInformation(
                "└─────────────────────────────────────────────────────────────────");
        }
    }
}
