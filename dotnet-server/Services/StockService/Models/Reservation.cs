namespace StockService.Models
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime ReservedAt { get; set; }
        public bool IsActive { get; set; } // для отката
    }
}
