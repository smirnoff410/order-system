using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using SharedKafkaEvents;
using SharedKafkaEvents.Events;
using SharedLibrary;
using StockService.Persistence;
using StockService.Services;
using System.Text.Json;

namespace StockService.Consumers
{
    public class PaymentFailedConsumer : SharedConsumer
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentFailedConsumer(IServiceProvider serviceProvider, IConfiguration config, ILogger<SharedConsumer> logger) 
            : base(
                config["Kafka:BootstrapServers"] ?? "empty_connect",
                KafkaConsumerGroups.StockService,
                KafkaTopics.PaymentFailed,
                logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<StockServiceContext>();
            var kafkaProducer = scope.ServiceProvider.GetRequiredService<KafkaProducerService>();

            var paymentEvent = JsonSerializer.Deserialize<PaymentFailedEvent>(consumeResult.Message.Value);

            var reservasions = await dbContext.Reservations.Where(x => x.OrderId == paymentEvent.OrderId).ToListAsync();
            foreach (var reservation in reservasions)
            {
                reservation.IsActive = false;

                var stockItem = await dbContext.StockItems.FirstAsync(x => x.ProductId == reservation.ProductId);
                stockItem.QuantityAvailable += reservation.Quantity;

            }
            await dbContext.SaveChangesAsync();
        }
    }
}
