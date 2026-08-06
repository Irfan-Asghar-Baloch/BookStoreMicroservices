using OrderService.DTO;
using OrderService.Entities;
using OrderService.Events;
using OrderService.Interface;
using OrderService.Interface.IOrderRepository;
using OrderService.Interface.IOrderService;

namespace OrderService.ServiceImplementation
{
    public class OrdersService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IBookApiService _bookApiService;   
        private readonly IRabbitMQPublisher _rabbitMQPublisher;
        public OrdersService(IOrderRepository orderRepository, IBookApiService bookApiService, IRabbitMQPublisher rabbitMQPublisher)
        {
            _orderRepository = orderRepository;
            _bookApiService = bookApiService;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        public async Task<ApiResponse<List<object>>> GetAllAsync()
        {
            var orders = await _orderRepository.GetAllAsync();

            var data = orders.Select(o => new
            {
                o.Id,
                o.UserId,
                o.TotalAmount,
                o.Status,
                o.CreatedOn,
                Items = o.OrderItems.Select(i => new
                {
                    i.Id,
                    i.BookId,
                    i.Quantity,
                    i.Price
                })
            }).ToList<object>();

            return new ApiResponse<List<object>>
            {
                Success = true,
                Message = "Orders fetched successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<object>> GetByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order not found."
                };
            }

            var data = new
            {
                order.Id,
                order.UserId,
                order.TotalAmount,
                order.Status,
                order.CreatedOn,
                Items = order.OrderItems.Select(i => new
                {
                    i.Id,
                    i.BookId,
                    i.Quantity,
                    i.Price
                })
            };

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Order fetched successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<string>> CreateAsync(CreateOrderRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Order must contain at least one item."
                };
            }

            var order = new Order
            {
                UserId = request.UserId,
                Status = "Pending"
            };

            decimal total = 0;

            foreach (var item in request.Items)
            {
                var book = await _bookApiService.GetBookByIdAsync(item.BookId);

                if (book == null)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"Book with Id {item.BookId} not found."
                    };
                }
                 if (item.Quantity <= 0)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Quantity must be greater than zero."
                    };
                }

                var orderItem = new OrderItem
                {
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    Price = book.Price  
                };
                total += book.Price * item.Quantity;
                order.OrderItems.Add(orderItem);
            }

            order.TotalAmount = total;

            await _orderRepository.AddAsync(order);
            var orderEvent = new OrderCreatedEvent
            {
                OrderId = order.Id,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                CreatedOn = order.CreatedOn
            };
            _rabbitMQPublisher.Publish(orderEvent);

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Order created successfully."
            };
        }
    }
}
