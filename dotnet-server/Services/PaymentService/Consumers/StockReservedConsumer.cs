using Confluent.Kafka;
using PaymentService.Services;
using SharedKafkaEvents;
using SharedKafkaEvents.Events;
using SharedLibrary;
using System.Text.Json;

namespace PaymentService.Consumers
{
    public class StockReservedConsumer : SharedConsumer
    {
        private readonly IServiceProvider _serviceProvider;

        public StockReservedConsumer(IServiceProvider serviceProvider, IConfiguration config, ILogger<SharedConsumer> logger) 
            : base(
                config["Kafka:BootstrapServers"] ?? "empty_connect",
                KafkaConsumerGroups.PaymentService,
                KafkaTopics.StockReserved,
                logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var kafkaProducer = scope.ServiceProvider.GetRequiredService<KafkaProducerService>();

            var stockEvent = JsonSerializer.Deserialize<StockReservedEvent>(consumeResult.Message.Value)
                ?? throw new InvalidOperationException("Unable to deserialize StockReservedEvent");

            var random = new Random();
            bool paymentSuccess = random.Next(1, 100) <= 80; // 80% успеха

            if (paymentSuccess)
            {
                await kafkaProducer.PublishPaymentCompletedAsync(new PaymentCompletedEvent
                {
                    OrderId = stockEvent.OrderId
                });
            }
            else
            {
                await kafkaProducer.PublishPaymentFailedAsync(new PaymentFailedEvent
                {
                    OrderId = stockEvent.OrderId,
                    Reason = "Payment failed"
                });
            }
        }
    }
}
