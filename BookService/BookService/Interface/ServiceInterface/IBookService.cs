using BookService.DTO;
using BookService.Entities;

namespace BookService.Interface.ServiceInterface
{
    public interface IBookService
    {
        Task<ApiResponse<List<Book>>> GetAllAsync();

        Task<ApiResponse<Book>> GetByIdAsync(int id);

        Task<ApiResponse<string>> AddAsync(AddBookRequest request);

        Task<ApiResponse<string>> UpdateAsync(UpdateBookRequest request);

        Task<ApiResponse<string>> DeleteAsync(int id);
    }
}
