namespace StockService.Models
{
    public class StockItem
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int QuantityAvailable { get; set; }
        public List<Reservation> Reservations { get; set; }
    }
}
