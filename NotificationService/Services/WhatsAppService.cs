using NotificationService.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NotificationService.Services
{
    public class WhatsAppService:IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public WhatsAppService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendTemplateAsync(string phoneNumber, string customerName,string purchaseType,string orderNumber, string productName, string deliveryDate)
        {
            var url =$"https://graph.facebook.com/v23.0/{_configuration["WhatsApp:PhoneNumberId"]}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = phoneNumber,
                type = "template",
                template = new
                {
                    name = "irfan_order_confirmed",
                    language = new
                    {
                        code = "en_US"
                    },
                    components = new[]
                    {
                new
                {
                    type = "body",
                    parameters = new object[]
                    {
                        new { type = "text", text = customerName },
                        new { type = "text", text = purchaseType },
                        new { type = "text", text = orderNumber },
                        new { type = "text", text = productName },
                        new { type = "text", text = deliveryDate }
                    }
                }
            }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    _configuration["WhatsApp:Token"]);

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);

            var result = await response.Content.ReadAsStringAsync();

            Console.WriteLine(result);

            response.EnsureSuccessStatusCode();
        }
        //public async Task SendAsync(string to, string message)
        //{
        //    var requestUri = _configuration["WhatsApp:ApiUrl"];
        //    var token = _configuration["WhatsApp:Token"];
        //    var payload = new
        //    {
        //        messaging_product = "whatsapp",
        //        to,
        //        type = "text",
        //        text = new { body = message }
        //    };
        //    var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
        //    {
        //        Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json")
        //    };
        //    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        //    var response = await _httpClient.SendAsync(requestMessage);

        //    var result = await response.Content.ReadAsStringAsync();

        //    Console.WriteLine(result);

        //    response.EnsureSuccessStatusCode();
        //}
    }
}
