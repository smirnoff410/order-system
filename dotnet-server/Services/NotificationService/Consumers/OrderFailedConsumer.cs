using Confluent.Kafka;
using SharedKafkaEvents.Events;
using SharedLibrary;
using System.Text.Json;

namespace NotificationService.Consumers
{
    public class OrderFailedConsumer : SharedConsumer
    {
        private readonly ILogger<OrderFailedConsumer> _logger;

        public OrderFailedConsumer(IConfiguration config, ILogger<OrderFailedConsumer> logger) 
            : base(config["Kafka:BootstrapServers"] ?? "empty_connect", "notification-service-group", "order-failed", logger)
        {
            _logger = logger;
        }

        protected override Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken stoppingToken)
        {
            var orderEvent = JsonSerializer.Deserialize<OrderFailedEvent>(consumeResult.Message.Value);

            _logger.LogInformation("✅ Order {OrderId} failed!", orderEvent.OrderId);

            return Task.CompletedTask;
        }
    }
}
