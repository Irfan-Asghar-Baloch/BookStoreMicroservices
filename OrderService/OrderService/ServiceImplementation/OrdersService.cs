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

        public async Task<ApiResponse> GetMyOrdersAsync(int userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);
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
            return new ApiResponse
            {
                Success = true,
                Message = "Orders fetched successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse> CancelAsync(int id, int requestingUserId, bool canAccessAny)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Order not found."
                };
            }
            if (!canAccessAny && order.UserId != requestingUserId)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "You do not have permission to cancel this order."
                };
            }
            if (order.Status == "Cancelled")
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Order is already cancelled."
                };
            }
            if(order.Status != "Pending")
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Order cannot be cancelled — current status is '{order.Status}'. Paid orders need a refund instead."
                };
            }
            await _orderRepository.UpdateStatusAsync(id, "Cancelled");
            return new ApiResponse
            {
                Success = true,
                Message = "Order cancelled successfully.",
                Data = new { id, status = "Cancelled" }
            };
        }

        public async Task<ApiResponse> GetByUserIdAsync(int userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);
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
            return new ApiResponse
            {
                Success = true,
                Message = "Orders fetched successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse> GetAllAsync()
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

            return new ApiResponse
            {
                Success = true,
                Message = "Orders fetched successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse> GetByIdAsync(int id, int requestingUserId, bool canAccessAny)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Order not found."
                };
            }
            if(!canAccessAny && order.UserId != requestingUserId)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "You do not have permission to access this order."
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

            return new ApiResponse
            {
                Success = true,
                Message = "Order fetched successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse> CreateAsync(CreateOrderRequest request, int userId)
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                return new ApiResponse { Success = false, Message = "Order must contain at least one item." };
            }

            var order = new Order
            {
                UserId = userId,
                Status = "Pending"
            };

            decimal total = 0;

            foreach (var item in request.Items)
            {
                var book = await _bookApiService.GetBookByIdAsync(item.BookId);
               Console.WriteLine($"DEBUG: BookId={book!.Price}");

                if (book == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = $"Book with Id {item.BookId} not found."
                    };
                }
                 if (item.Quantity <= 0)
                {
                    return new ApiResponse
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

            return new ApiResponse
            {
                Success = true,
                Message = "Order created successfully."
            };
        }
    }
}
