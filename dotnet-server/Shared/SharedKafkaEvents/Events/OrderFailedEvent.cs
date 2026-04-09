namespace SharedKafkaEvents.Events
{
    public class OrderFailedEvent
    {
        public Guid OrderId { get; set; }
        public List<FailedItemEvent> FailedItems { get; set; }
    }
}
