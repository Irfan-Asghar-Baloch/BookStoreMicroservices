using OrderService.DTO;
using Stripe;

namespace OrderService.Interface
{
    public interface IPaymentService
    {
        Task<PaymentResponse> CreatePaymentIntentAsync(CreatePaymentRequest request);
        Task<PaymentIntent> ConfirmPaymentAsync(ConfirmPaymentRequest request);
    }
}
