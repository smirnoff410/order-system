using Confluent.Kafka;
using SharedKafkaEvents.Events;
using System.Text;
using System.Text.Json;

namespace OrderService.Services
{
    public class KafkaProducerService
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;
        private const string OrderCreatedTopic = "order-created";

        public KafkaProducerService(IConfiguration config, ILogger<KafkaProducerService> logger)
        {
            _logger = logger;
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                ClientId = "order-service",
                // Idempotence для гарантии доставки
                EnableIdempotence = true,
                Acks = Acks.All
            };

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        public async Task PublishOrderCreatedAsync(OrderCreatedEvent orderEvent)
        {
            try
            {
                var json = JsonSerializer.Serialize(orderEvent);
                var message = new Message<string, string>
                {
                    Key = orderEvent.OrderId.ToString(),
                    Value = json,
                    Headers = new Headers
                {
                    { "event-type", Encoding.UTF8.GetBytes("OrderCreated") },
                    { "timestamp", BitConverter.GetBytes(DateTime.UtcNow.Ticks) }
                }
                };

                var result = await _producer.ProduceAsync(OrderCreatedTopic, message);
                _logger.LogInformation("Published OrderCreated: {OrderId} to partition {Partition}, offset {Offset}",
                    orderEvent.OrderId, result.Partition, result.Offset);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to publish OrderCreated: {OrderId}", orderEvent.OrderId);
                throw;
            }
        }
    }
}
