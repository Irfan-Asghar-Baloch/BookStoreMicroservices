using OrderService.DTO;
using OrderService.Interface;
using OrderService.Interface.IOrderRepository;
using Stripe;

namespace OrderService.ServiceImplementation
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepository;

        public PaymentService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<PaymentResponse> CreatePaymentIntentAsync(CreatePaymentRequest request)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);

            if (order == null)
                throw new Exception("Order not found.");

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(order.TotalAmount * 100),
                Currency = "usd",

                Description = $"Payment for Order #{order.Id}",

                PaymentMethodTypes = new List<string>
                {
                    "card"
                },

                Metadata = new Dictionary<string, string>
                {
                    { "OrderId", order.Id.ToString() },
                    { "UserId", order.UserId.ToString() }
                }
            };

            var service = new PaymentIntentService();

            var paymentIntent = await service.CreateAsync(options);

            return new PaymentResponse
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret
            };
        }

        public async Task<PaymentIntent> ConfirmPaymentAsync(ConfirmPaymentRequest request)
        {
            var service = new PaymentIntentService();

            var options = new PaymentIntentConfirmOptions
            {
                PaymentMethod = request.PaymentMethodId
            };

            return await service.ConfirmAsync(request.PaymentIntentId, options);
        }
    }
}