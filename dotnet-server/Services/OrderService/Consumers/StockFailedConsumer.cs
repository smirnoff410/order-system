using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using OrderService.Persistence;
using OrderService.Services;
using SharedKafkaEvents.Events;
using SharedLibrary;
using System.Text.Json;

namespace OrderService.Consumers
{
    public class StockFailedConsumer : SharedConsumer
    {
        private readonly IServiceProvider _serviceProvider;

        public StockFailedConsumer(IServiceProvider serviceProvider, IConfiguration config, ILogger<SharedConsumer> logger) 
            : base(config["Kafka:BootstrapServers"] ?? "empty_connect", "order-service-group", "stock-failed", logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderServiceContext>();
            var kafkaProducer = scope.ServiceProvider.GetRequiredService<KafkaProducerService>();

            var stockEvent = JsonSerializer.Deserialize<StockFailedEvent>(consumeResult.Message.Value);

            var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == stockEvent.OrderId);
            if (order != null)
            {
                order.Status = "Failed";
                order.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
            }

            await kafkaProducer.PublishOrderFaileddAsync(new OrderFailedEvent { OrderId = stockEvent.OrderId });
        }
    }
}
