namespace StockService.Requests
{
    public class AddStockItemRequest
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}
