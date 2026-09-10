using Microsoft.AspNetCore.Mvc;
using OrderService.DTO;
using OrderService.Interface;
using OrderService.Interface.IOrderRepository;
using Stripe;

namespace OrderService.ServiceImplementation
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;
        public PaymentService(IOrderRepository orderRepository, IPaymentRepository paymentRepository)
        {
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<ApiResponse> RefundAsync(int orderId, int requestingUserId, bool canAccessAny)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return new ApiResponse { Success = false, Message = "Order not found." };

            if (canAccessAny && order.UserId != requestingUserId)
                return new ApiResponse { Success = false, Message = "Access denied." };

            if (order.Status != "Paid")
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Order cannot be refunded — current status is '{order.Status}'."
                };

            var payment = await _paymentRepository.GetPaymentByOrderIdAsync(orderId);
            if (payment == null)
                return new ApiResponse { Success = false, Message = "No payment record found for this order." };

            var refundService = new RefundService();
            var refund = await refundService.CreateAsync(new RefundCreateOptions
            {
                PaymentIntent = payment.PaymentIntentId
            });

            await _orderRepository.UpdateStatusAsync(orderId, "Refunded");

            return new ApiResponse
            {
                Success = true,
                Message = "Refund processed successfully.",
                Data = new { orderId, refundId = refund.Id, status = refund.Status }
            };
        }
        public async Task<PaymentResponse> CreatePaymentIntentAsync([FromQuery] int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order == null)
                throw new Exception("Order not found.");

            var originalAmount = order.TotalAmount;

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(originalAmount * 100),
                Currency = "usd",

                Description = $"Payment for Order #{order.Id}",

                PaymentMethodTypes = new List<string>
    {
        "card"
    },

                Metadata = new Dictionary<string, string>
    {
        { "OrderId", order.Id.ToString() },
        { "UserId", order.UserId.ToString() },
        { "OriginalAmount", originalAmount.ToString() }
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