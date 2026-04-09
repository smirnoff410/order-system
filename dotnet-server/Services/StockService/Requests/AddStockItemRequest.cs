namespace StockService.Requests
{
    public class AddStockItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
