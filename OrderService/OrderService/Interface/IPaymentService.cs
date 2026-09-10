using OrderService.DTO;
using Stripe;

namespace OrderService.Interface
{
    public interface IPaymentService
    {
        Task<PaymentResponse> CreatePaymentIntentAsync(int orderId);
        Task<PaymentIntent> ConfirmPaymentAsync(ConfirmPaymentRequest request);
        Task<ApiResponse> RefundAsync(int orderId, int requestingUserId, bool canAccessAny);
    }
}
