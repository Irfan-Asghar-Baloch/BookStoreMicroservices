using OrderService.Events;
using OrderService.Interface;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace OrderService.Messaging
{
    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        private readonly IConfiguration _configuration;

        public RabbitMQPublisher(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Publish(OrderCreatedEvent orderEvent)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:HostName"]
            };

             var connection =   factory.CreateConnection();
            var channel =  connection.CreateModel();

            channel.QueueDeclare(
                queue: "payment-success",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var message = JsonSerializer.Serialize(orderEvent);

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
                exchange: "",
                routingKey: "payment-success",
                basicProperties: null,
                body: body);

            Debug.WriteLine($"Message Published : {message}");
        }

        public void PublishPaymentSuccess(PaymentSuccessEvent paymentEvent)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:HostName"]
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(
                queue: "payment-success",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            var message = JsonSerializer.Serialize(paymentEvent);
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(
                exchange: "",
                routingKey: "payment-success",
                basicProperties: null,
                body: body);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Payment Success Published: {message}");
            Console.ResetColor();
            // Debug.WriteLine($"Payment Success Message Published : {message}");
        }
    }
}