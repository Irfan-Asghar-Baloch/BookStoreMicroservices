using OrderService.DTO;

namespace OrderService.Interface.IOrderService
{
    public interface IOrderService
    {
        Task<ApiResponse<List<object>>> GetAllAsync();
        Task<ApiResponse<object>> GetByIdAsync(int id);
        Task<ApiResponse<string>> CreateAsync(CreateOrderRequest request);
    }
}
