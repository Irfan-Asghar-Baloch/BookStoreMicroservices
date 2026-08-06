namespace OrderService.Events
{
    public class OrderCreatedEvent
    {
            public int OrderId { get; set; }
            public int UserId { get; set; }
            public decimal TotalAmount { get; set; }
            public DateTime CreatedOn { get; set; }
    }
}
