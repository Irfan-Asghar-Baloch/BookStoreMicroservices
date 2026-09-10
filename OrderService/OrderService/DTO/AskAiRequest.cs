namespace OrderService.DTO
{
    public class AskAiRequest
    {
        public string Message { get; set; } = string.Empty;
        public List<ChatTurn> History { get; set; } = new();
    }
   
   public class ChatTurn
    {
       public string Role { get; set; } = string.Empty;
      public string Text { get; set; } = string.Empty;
    }
}
