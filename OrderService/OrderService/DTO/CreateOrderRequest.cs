namespace OrderService.DTO
{
    public class CreateOrderRequest
    {
        public List<OrderItemRequest> Items { get; set; } = new();
    }
}
