using OrderService.Events;

namespace OrderService.Interface
{
    public interface IRabbitMQPublisher
    {
        void Publish(OrderCreatedEvent orderEvent);
        void PublishPaymentSuccess(PaymentSuccessEvent paymentEvent);
    }
}
