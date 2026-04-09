using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using OrderService.Persistence;
using OrderService.Services;
using SharedKafkaEvents.Events;
using System.Text.Json;

namespace OrderService.Consumers
{
    public class PaymentCompletedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentCompletedConsumer> _logger;
        private readonly IConsumer<string, string> _consumer;

        public PaymentCompletedConsumer(IConfiguration config, IServiceProvider serviceProvider, ILogger<PaymentCompletedConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                GroupId = "order-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false, // Ручной commit для гарантии
                EnableAutoOffsetStore = false
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            _consumer.Subscribe("payment-completed");
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
