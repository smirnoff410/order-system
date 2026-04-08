namespace SharedKafkaEvents.Events
{
    public class StockReservedEvent
    {
        public Guid OrderId { get; set; }
        public List<ReservedItemEvent> ReservedItems { get; set; }
    }

    public class ReservedItemEvent
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
