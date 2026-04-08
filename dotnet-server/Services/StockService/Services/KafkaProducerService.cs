using Confluent.Kafka;
using SharedKafkaEvents.Events;
using System.Text;
using System.Text.Json;

namespace StockService.Services
{
    public class KafkaProducerService
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;
        private const string StockReservedTopic = "stock-reserved";
        private const string StockFailedTopic = "stock-failed";

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

        public async Task PublishStockReservedAsync(StockReservedEvent stockReservedEvent)
        {
            try
            {
                var json = JsonSerializer.Serialize(stockReservedEvent);
                var message = new Message<string, string>
                {
                    Key = stockReservedEvent.OrderId.ToString(),
                    Value = json,
                    Headers = new Headers
                {
                    { "event-type", Encoding.UTF8.GetBytes("StockReserved") },
                    { "timestamp", BitConverter.GetBytes(DateTime.UtcNow.Ticks) }
                }
                };

                var result = await _producer.ProduceAsync(StockReservedTopic, message);
                _logger.LogInformation("Published StockReserved: {OrderId} to partition {Partition}, offset {Offset}",
                    stockReservedEvent.OrderId, result.Partition, result.Offset);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to publish StockReserved: {OrderId}", stockReservedEvent.OrderId);
                throw;
            }
        }

        public async Task PublishStockFailedAsync(StockFailedEvent stockFailedEvent)
        {
            try
            {
                var json = JsonSerializer.Serialize(stockFailedEvent);
                var message = new Message<string, string>
                {
                    Key = stockFailedEvent.OrderId.ToString(),
                    Value = json,
                    Headers = new Headers
                {
                    { "event-type", Encoding.UTF8.GetBytes("StockFailed") },
                    { "timestamp", BitConverter.GetBytes(DateTime.UtcNow.Ticks) }
                }
                };

                var result = await _producer.ProduceAsync(StockFailedTopic, message);
                _logger.LogInformation("Published StockFailed: {OrderId} to partition {Partition}, offset {Offset}",
                    stockFailedEvent.OrderId, result.Partition, result.Offset);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to publish StockFailed: {OrderId}", stockFailedEvent.OrderId);
                throw;
            }
        }
    }
}
