using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using SharedKafkaEvents.Events;
using SharedLibrary;
using StockService.Models;
using StockService.Persistence;
using StockService.Services;
using System.Text.Json;

namespace StockService.Consumers
{
    public class OrderCreatedConsumer : SharedConsumer
    {
        private readonly IServiceProvider _serviceProvider;

        public OrderCreatedConsumer(IServiceProvider serviceProvider, IConfiguration config, ILogger<SharedConsumer> logger) 
            : base(config["Kafka:BootstrapServers"] ?? "empty_connect", "stock-service-group", "order-created", logger)
        {
            _serviceProvider = serviceProvider;
        }
        protected async override Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<StockServiceContext>();
            var kafkaProducer = scope.ServiceProvider.GetRequiredService<KafkaProducerService>();

            // Парсим событие
            var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(consumeResult.Message.Value);

            // Проверяем наличие товаров
            var canReserve = true;
            var failedItems = new List<FailedItemEvent>();

            foreach (var item in orderEvent.Items)
            {
                var stock = await dbContext.StockItems
                    .Include(x => x.Reservations)
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                if (stock == null || stock.QuantityAvailable < item.Quantity)
                {
                    canReserve = false;
                    failedItems.Add(new FailedItemEvent
                    {
                        ProductId = item.ProductId,
                        RequestedQuantity = item.Quantity,
                        AvailableQuantity = stock?.QuantityAvailable ?? 0
                    });
                }
            }

            if (canReserve)
            {
                // Резервируем товары
                foreach (var item in orderEvent.Items)
                {
                    var stock = await dbContext.StockItems
                        .FirstAsync(s => s.ProductId == item.ProductId);

                    stock.QuantityAvailable -= item.Quantity;
                    stock.Reservations.Add(new Reservation
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderEvent.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        ReservedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                }

                await dbContext.SaveChangesAsync();

                // Публикуем успех
                await kafkaProducer.PublishStockReservedAsync(new StockReservedEvent
                {
                    OrderId = orderEvent.OrderId,
                    ReservedItems = orderEvent.Items.Select(i => new ReservedItemEvent
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                });
            }
            else
            {
                // Публикуем неудачу
                await kafkaProducer.PublishStockFailedAsync(new StockFailedEvent
                {
                    OrderId = orderEvent.OrderId,
                    Reason = "Insufficient stock",
                    FailedItems = failedItems
                });
            }
        }
    }
}
