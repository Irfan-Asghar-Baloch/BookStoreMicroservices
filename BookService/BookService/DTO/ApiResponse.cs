namespace BookService.DTO
{
    public class ApiResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public Object? Data { get; set; }
    }
}
