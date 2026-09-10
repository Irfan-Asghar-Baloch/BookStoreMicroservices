using OrderService.Entities;

namespace OrderService.Interface.IOrderRepository
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int id);
        Task AddAsync(Order order);
        Task<List<Order>> GetByUserIdAsync(int userId);
        Task UpdateStatusAsync(int orderId, string status);
    }
}
