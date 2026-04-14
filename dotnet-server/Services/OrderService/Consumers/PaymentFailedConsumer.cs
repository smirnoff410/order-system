using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using OrderService.Persistence;
using OrderService.Services;
using SharedKafkaEvents.Events;
using SharedLibrary;
using System.Text.Json;

namespace OrderService.Consumers
{
    public class PaymentFailedConsumer : SharedConsumer
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentFailedConsumer(IServiceProvider serviceProvider, IConfiguration config, ILogger<SharedConsumer> logger) 
            : base(config["Kafka:BootstrapServers"] ?? "empty_connect", "order-service-group", "payment-failed", logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderServiceContext>();
            var kafkaProducer = scope.ServiceProvider.GetRequiredService<KafkaProducerService>();

            var paymentEvent = JsonSerializer.Deserialize<PaymentFailedEvent>(consumeResult.Message.Value);

            var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == paymentEvent.OrderId);
            if (order != null)
            {
                order.Status = "PaymentFailed";
                order.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
            }

            await kafkaProducer.PublishOrderFaileddAsync(new OrderFailedEvent { OrderId = paymentEvent.OrderId });
        }
    }
}
