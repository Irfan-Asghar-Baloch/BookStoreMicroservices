using OrderService.Entities;

namespace OrderService.Interface
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId);
        Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
    }
}
