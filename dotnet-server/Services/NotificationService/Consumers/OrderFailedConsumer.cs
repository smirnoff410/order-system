using Confluent.Kafka;
using SharedKafkaEvents.Events;
using System.Text.Json;

namespace NotificationService.Consumers
{
    public class OrderFailedConsumer : BackgroundService
    {
        private readonly ILogger<OrderFailedConsumer> _logger;
        private readonly IConsumer<string, string> _consumer;

        public OrderFailedConsumer(IConfiguration config, ILogger<OrderFailedConsumer> logger)
        {
            _logger = logger;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                GroupId = "notification-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false, // Ручной commit для гарантии
                EnableAutoOffsetStore = false
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            _consumer.Subscribe("order-failed");
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
            var orderEvent = JsonSerializer.Deserialize<OrderFailedEvent>(consumeResult.Message.Value);

            _logger.LogInformation("✅ Order {OrderId} failed!", orderEvent.OrderId);
            // Можно отправить email через SMTP или Telegram бота
        }
    }
}
