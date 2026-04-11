using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using OrderService.Persistence;
using OrderService.Services;
using SharedKafkaEvents.Events;
using SharedLibrary;
using System.Text.Json;

namespace OrderService.Consumers
{
    public class PaymentCompletedConsumer : SharedConsumer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentCompletedConsumer> _logger;

        public PaymentCompletedConsumer(IServiceProvider serviceProvider, IConfiguration config, ILogger<PaymentCompletedConsumer> logger) 
            : base(config["Kafka:BootstrapServers"] ?? "empty_connect", "order-service-group", "payment-completed", logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderServiceContext>();
            var kafkaProducer = scope.ServiceProvider.GetRequiredService<KafkaProducerService>();

            var paymentEvent = JsonSerializer.Deserialize<PaymentCompletedEvent>(consumeResult.Message.Value);

            _logger.LogInformation("✅ Payment by order {OrderId} completed successfully!", paymentEvent?.OrderId);

            var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == paymentEvent.OrderId);
            if (order != null)
            {
                order.Status = "Completed";
                order.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();

                await kafkaProducer.PublishOrderCompletedAsync(new OrderCompletedEvent { OrderId = order.Id });
            }
            else
            {
                _logger.LogInformation("Order with id {OrderId} not found!", paymentEvent?.OrderId);

                await kafkaProducer.PublishOrderFaileddAsync(new OrderFailedEvent { OrderId = paymentEvent.OrderId });
            }
        }
    }
}
