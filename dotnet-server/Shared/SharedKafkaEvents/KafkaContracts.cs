namespace SharedKafkaEvents
{
    public static class KafkaTopics
    {
        public const string OrderCreated = "order-created";
        public const string StockReserved = "stock-reserved";
        public const string StockFailed = "stock-failed";
        public const string PaymentCompleted = "payment-completed";
        public const string PaymentFailed = "payment-failed";
        public const string OrderCompleted = "order-completed";
        public const string OrderFailed = "order-failed";
    }

    public static class KafkaConsumerGroups
    {
        public const string OrderService = "order-service-group";
        public const string StockService = "stock-service-group";
        public const string PaymentService = "payment-service-group";
        public const string NotificationService = "notification-service-group";
    }
}
