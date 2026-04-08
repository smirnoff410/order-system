namespace SharedKafkaEvents.Events
{
    public class FailedItemEvent
    {
        public Guid ProductId { get; set; }
        public int RequestedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
    }
}
