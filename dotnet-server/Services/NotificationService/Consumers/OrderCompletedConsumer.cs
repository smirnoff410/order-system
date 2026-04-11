using Confluent.Kafka;
using SharedKafkaEvents.Events;
using SharedLibrary;
using System.Text.Json;

namespace NotificationService.Consumers
{
    public class OrderCompletedConsumer : SharedConsumer
    {
        private readonly ILogger<OrderCompletedConsumer> _logger;

        public OrderCompletedConsumer(IConfiguration config, ILogger<OrderCompletedConsumer> logger) 
            : base(config["Kafka:BootstrapServers"] ?? "empty_connect", "notification-service-group", "order-completed", logger)
        {
            _logger = logger;
        }

        protected override Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken stoppingToken)
        {
            var orderEvent = JsonSerializer.Deserialize<OrderCompletedEvent>(consumeResult.Message.Value);

            _logger.LogInformation("✅ Order {OrderId} completed successfully!", orderEvent.OrderId);

            return Task.CompletedTask;
        }
    }
}
