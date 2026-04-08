using Confluent.Kafka;
using SharedKafkaEvents.Events;
using System.Text;
using System.Text.Json;

namespace PaymentService.Services
{
    public class KafkaProducerService
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;
        private const string PaymentCompletedTopic = "payment-completed";
        private const string PaymentFailedTopic = "stock-failed";

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

        public async Task PublishPaymentCompletedAsync(PaymentCompletedEvent paymentCompletedEvent)
        {
            try
            {
                var json = JsonSerializer.Serialize(paymentCompletedEvent);
                var message = new Message<string, string>
                {
                    Key = paymentCompletedEvent.OrderId.ToString(),
                    Value = json,
                    Headers = new Headers
                {
                    { "event-type", Encoding.UTF8.GetBytes("PaymentCompleted") },
                    { "timestamp", BitConverter.GetBytes(DateTime.UtcNow.Ticks) }
                }
                };

                var result = await _producer.ProduceAsync(PaymentCompletedTopic, message);
                _logger.LogInformation("Published PaymentCompleted: {OrderId} to partition {Partition}, offset {Offset}",
                    paymentCompletedEvent.OrderId, result.Partition, result.Offset);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to publish PaymentCompleted: {OrderId}", paymentCompletedEvent.OrderId);
                throw;
            }
        }

        public async Task PublishPaymentFailedAsync(PaymentFailedEvent paymentEvent)
        {
            try
            {
                var json = JsonSerializer.Serialize(paymentEvent);
                var message = new Message<string, string>
                {
                    Key = paymentEvent.OrderId.ToString(),
                    Value = json,
                    Headers = new Headers
                {
                    { "event-type", Encoding.UTF8.GetBytes("PaymentFailed") },
                    { "timestamp", BitConverter.GetBytes(DateTime.UtcNow.Ticks) }
                }
                };

                var result = await _producer.ProduceAsync(PaymentFailedTopic, message);
                _logger.LogInformation("Published PaymentFailed: {OrderId} to partition {Partition}, offset {Offset}",
                    paymentEvent.OrderId, result.Partition, result.Offset);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to publish PaymentFailed: {OrderId}", paymentEvent.OrderId);
                throw;
            }
        }
    }
}
