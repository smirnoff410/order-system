namespace NotificationService.Consumers
{
    public class OrderCompletedConsumer : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Подписывается на "order-completed" и "order-failed"
            // Логирует в консоль/файл
            //_logger.LogInformation("✅ Order {OrderId} completed successfully!", orderId);
            // Можно отправить email через SMTP или Telegram бота
        }
    }
}
