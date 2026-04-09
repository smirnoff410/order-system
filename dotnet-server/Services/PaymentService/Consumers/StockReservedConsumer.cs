using Confluent.Kafka;
using PaymentService.Services;
using SharedKafkaEvents.Events;
using System.Text.Json;

namespace PaymentService.Consumers
{
    public class StockReservedConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StockReservedConsumer> _logger;

        public StockReservedConsumer(IConfiguration config, IServiceProvider serviceProvider, ILogger<StockReservedConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                GroupId = "payment-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false, // Ручной commit для гарантии
                EnableAutoOffsetStore = false
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            _consumer.Subscribe("stock-reserved");
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
