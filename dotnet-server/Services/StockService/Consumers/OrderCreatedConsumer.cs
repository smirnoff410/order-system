using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using SharedKafkaEvents.Events;
using StockService.Models;
using StockService.Persistence;
using StockService.Services;
using System.Text.Json;

namespace StockService.Consumers
{
    public class OrderCreatedConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderCreatedConsumer> _logger;

        public OrderCreatedConsumer(IConfiguration config, IServiceProvider serviceProvider, ILogger<OrderCreatedConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                GroupId = "stock-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false, // Ручной commit для гарантии
                EnableAutoOffsetStore = false
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            _consumer.Subscribe("order-created");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => ProcessMessages(stoppingToken), stoppingToken);
        }
        private async Task ProcessMessages(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Асинхронный Consume (не блокирует)
                    var consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult != null && consumeResult.Message != null)
                    {
                        // Обработка сообщения
                        await ProcessMessageAsync(consumeResult, stoppingToken);
                        _consumer.Commit(consumeResult);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Consumer stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming message");
                    await Task.Delay(1000, stoppingToken);
                }
            }

            _consumer.Close();
        }
        private async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken stoppingToken)
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
