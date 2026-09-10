using OrderService.DTO;
using OrderService.Interface;

namespace OrderService.ServiceImplementation
{
    public class AiAgentService : IAiAgentService
    {
        private readonly HttpClient _httpClient;
        public AiAgentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> AskAsync(string message, List<ChatTurn> history)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("ask-agent", new { message, history });
                if (response == null)
                {
                    return null;
                }
                var result = await response.Content.ReadFromJsonAsync<AskAiResponse>();
                return result?.Answer;
            }
            catch (HttpRequestException)
            {
                // FastAPI unreachable/down
                return null;
            }
            catch (TaskCanceledException)
            {
                // Timeout
                return null;
            }
        }
    }
}
