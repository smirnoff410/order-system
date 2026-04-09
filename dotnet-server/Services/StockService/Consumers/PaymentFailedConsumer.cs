using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using SharedKafkaEvents.Events;
using StockService.Persistence;
using StockService.Services;
using System.Text.Json;

namespace StockService.Consumers
{
    public class PaymentFailedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentFailedConsumer> _logger;
        private readonly IConsumer<string, string> _consumer;

        public PaymentFailedConsumer(IConfiguration config, IServiceProvider serviceProvider, ILogger<PaymentFailedConsumer> logger)
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
            _consumer.Subscribe("payment-failed");
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
