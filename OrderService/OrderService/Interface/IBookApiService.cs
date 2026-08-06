using OrderService.DTO;

namespace OrderService.Interface
{
    public interface IBookApiService
    {
        Task<BookResponse?> GetBookByIdAsync(int bookId);
    }
}
