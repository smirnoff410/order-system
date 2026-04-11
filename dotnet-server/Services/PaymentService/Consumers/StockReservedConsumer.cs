using Confluent.Kafka;
using PaymentService.Services;
using SharedKafkaEvents.Events;
using SharedLibrary;
using System.Text.Json;

namespace PaymentService.Consumers
{
    public class StockReservedConsumer : SharedConsumer
    {
        private readonly IServiceProvider _serviceProvider;

        public StockReservedConsumer(IServiceProvider serviceProvider, IConfiguration config, ILogger<SharedConsumer> logger) 
            : base(config["Kafka:BootstrapServers"] ?? "empty_connect", "payment-service-group", "stock-reserved", logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var kafkaProducer = scope.ServiceProvider.GetRequiredService<KafkaProducerService>();

            var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(consumeResult.Message.Value);

            var random = new Random();
            bool paymentSuccess = random.Next(1, 100) <= 80; // 80% успеха

            if (paymentSuccess)
            {
                await kafkaProducer.PublishPaymentCompletedAsync(new PaymentCompletedEvent
                {
                    OrderId = orderEvent.OrderId
                });
            }
            else
            {
                await kafkaProducer.PublishPaymentFailedAsync(new PaymentFailedEvent
                {
                    OrderId = orderEvent.OrderId,
                    Reason = "Payment failed"
                });
            }
        }
    }
}
