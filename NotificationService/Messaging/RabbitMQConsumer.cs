using Microsoft.Extensions.Hosting;
using NotificationService.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NotificationService.Messaging
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IWhatsAppService _whatsAppService;

        public RabbitMQConsumer(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            IWhatsAppService whatsAppService)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _whatsAppService = whatsAppService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("RabbitMQ Consumer Started");
            Console.ResetColor();

            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:HostName"]
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: "payment-success",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            Console.WriteLine("Listening Queue : payment-success");

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("===================================");
                Console.WriteLine("Message Received");
                Console.WriteLine(message);
                Console.WriteLine("===================================");
                Console.ResetColor();

                using var scope = _scopeFactory.CreateScope();

                var emailService = scope.ServiceProvider
                    .GetRequiredService<IEmailService>();

                await emailService.SendAsync(
                    "irfankkhan255@gmail.com", // Replace with actual email later
                    "Payment Successful",
                    "Your payment has been received successfully.");
                Console.WriteLine("Email Sent Successfully");
                //await _whatsAppService.SendAsync(
                //    "+923336493781", // Replace with actual WhatsApp number later
                //    "Your payment has been received successfully.");    
                   await _whatsAppService.SendTemplateAsync(
                    "923336493781",
                    "Irfan",
                    "Purchase",
                    "#12345",
                    "Book Store Order",
                    DateTime.Now.AddDays(2).ToString("dd MMM yyyy"));
                Console.WriteLine("whatsapp Sent Successfully");
            };

            channel.BasicConsume(
                queue: "payment-success",
                autoAck: true,
                consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}