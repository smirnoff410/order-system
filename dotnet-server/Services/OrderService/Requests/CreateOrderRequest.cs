namespace OrderService.Requests
{
    public class CreateOrderRequest
    {
        public string CustomerName { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; }
    }
    public class CreateOrderItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public int UnitPrice { get; set; }
    }
}
