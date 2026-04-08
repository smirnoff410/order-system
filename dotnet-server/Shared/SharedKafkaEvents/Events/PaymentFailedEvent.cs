namespace SharedKafkaEvents.Events
{
    public class PaymentFailedEvent
    {
        public Guid OrderId { get; set; }
        public string Reason { get; set; }
        public List<FailedItemEvent> FailedItems { get; set; }
    }
}
