using OrderService.DTO;
using OrderService.Interface;

namespace OrderService.ServiceImplementation
{
    public class BookApiService:IBookApiService
    {
        private readonly HttpClient _httpClient;
        public BookApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<BookResponse?> GetBookByIdAsync(int id)
        {
            var response = await _httpClient
                .GetFromJsonAsync<ApiResponse<BookResponse>>($"api/Book/{id}");

            if (response == null || !response.Success)
                return null;

            return response.Data;
        }
    }
}
