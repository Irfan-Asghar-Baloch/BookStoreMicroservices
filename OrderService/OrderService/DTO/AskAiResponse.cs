using System.Text.Json.Serialization;

namespace OrderService.DTO
{
    public class AskAiResponse
    {
        [JsonPropertyName("answer")]
        public string Answer { get; set; } = string.Empty;
    }
}