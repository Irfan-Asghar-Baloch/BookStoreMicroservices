using OrderService.DTO;

namespace OrderService.Interface
{
    public interface IAiAgentService
    {
        Task<string?> AskAsync(string message, List<ChatTurn> history);
    }
}