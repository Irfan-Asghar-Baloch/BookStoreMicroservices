using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTO;
using OrderService.Entities;
using OrderService.Events;
using OrderService.Interface;
using OrderService.Interface.IOrderRepository;
using Stripe;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRabbitMQPublisher _rabbitMQPublisher;
        public PaymentController(IPaymentService paymentService, IConfiguration configuration, IPaymentRepository paymentRepository , IOrderRepository orderRepository, IRabbitMQPublisher rabbitMQPublisher)
        {
            _paymentService = paymentService;
            _configuration = configuration;
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _rabbitMQPublisher = rabbitMQPublisher;
            _orderRepository = orderRepository;
        }

        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent(CreatePaymentRequest request)
        {

            var paymentResponse = await _paymentService.CreatePaymentIntentAsync(request);
           // await _orderRepository.UpdateStatusAsync(request.OrderId, "Paid");
            Console.WriteLine("Order Updated Successfully");
            return Ok(paymentResponse);
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment(ConfirmPaymentRequest request)
        {
            var result = await _paymentService.ConfirmPaymentAsync(request);

            return Ok(new
            {
                result.Id,
                result.Status,
                result.Amount,
                result.Currency
            });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            try
            {
                Console.WriteLine("===== WEBHOOK HIT =====");

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _configuration["Stripe:WebhookSecret"]);

                Console.WriteLine($"Event Type : {stripeEvent.Type}");

                switch (stripeEvent.Type)
                {
                    case "payment_intent.created":
                        Console.WriteLine("Payment Intent Created");
                        break;

                    case "payment_intent.succeeded":

                        Console.WriteLine("Payment Intent Succeeded");

                        var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;
                        var existingPayment = await _paymentRepository.GetByPaymentIntentIdAsync(paymentIntent.Id);
                        if (existingPayment != null)
                        {
                            Console.WriteLine("Payment Already Exists");
                            return Ok();
                        }
                        // Debug
                        Console.WriteLine($"PaymentIntentId : {paymentIntent.Id}");
                        Console.WriteLine($"Amount : {paymentIntent.Amount}");
                        Console.WriteLine($"Status : {paymentIntent.Status}");

                        if (!paymentIntent.Metadata.ContainsKey("OrderId"))
                        {
                            Console.WriteLine("OrderId not found in Metadata.");
                            return Ok();   // App crash nahi karegi
                        }

                        int orderId = int.Parse(paymentIntent.Metadata["OrderId"]);

                        var payment = new Payment
                        {
                            OrderId = orderId,
                            PaymentIntentId = paymentIntent.Id,
                            Amount = paymentIntent.Amount / 100m,
                            Currency = paymentIntent.Currency,
                            Status = paymentIntent.Status,
                            PaidOn = DateTime.UtcNow
                        };

                        await _paymentRepository.AddAsync(payment);

                        Console.WriteLine("Payment Saved Successfully");

                        await _orderRepository.UpdateStatusAsync(orderId, "Paid");

                        Console.WriteLine("Order Updated Successfully");
                        _rabbitMQPublisher.PublishPaymentSuccess(new PaymentSuccessEvent
                        {
                            OrderId = orderId,
                            UserId = int.Parse(paymentIntent.Metadata["UserId"]),
                            Amount = payment.Amount,
                            Currency = paymentIntent.Currency,
                            Status = paymentIntent.Status
                        });

                        break;
                    case "payment_intent.payment_failed":

                        Console.WriteLine("Payment Failed");

                        var failedIntent = (PaymentIntent)stripeEvent.Data.Object;

                        if (failedIntent.Metadata.ContainsKey("OrderId"))
                        {
                            int orderid = int.Parse(failedIntent.Metadata["OrderId"]);

                            await _orderRepository.UpdateStatusAsync(orderid, "Payment Failed");

                            Console.WriteLine("Order Updated -> Payment Failed");
                        }

                        break;

                    default:
                        Console.WriteLine($"Unhandled Event : {stripeEvent.Type}");
                        break;
                }

                Console.WriteLine("===== WEBHOOK END =====");

                return Ok();
            }
            catch (StripeException ex)
            {
                Console.WriteLine("Stripe Exception");
                Console.WriteLine(ex.Message);

                return BadRequest();
            }
            catch (Exception ex)
            {
                Console.WriteLine("General Exception");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return StatusCode(500);
            }
        }
    }
}
