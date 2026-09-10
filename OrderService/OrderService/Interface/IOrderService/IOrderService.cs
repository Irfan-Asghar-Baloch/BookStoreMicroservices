using OrderService.DTO;
using OrderService.Entities;

namespace OrderService.Interface.IOrderService
{
    public interface IOrderService
    {
        Task<ApiResponse> GetAllAsync();
        Task<ApiResponse> CancelAsync(int id, int requestingUserId, bool canAccessAny);
        Task<ApiResponse> GetByUserIdAsync(int userId);
        Task<ApiResponse> CreateAsync(CreateOrderRequest request, int userId);
        Task<ApiResponse> GetMyOrdersAsync(int userId);
        Task<ApiResponse> GetByIdAsync(int id, int requestingUserId, bool canAccessAny);
    }
}
