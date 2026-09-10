using BookService.DTO;
using BookService.Entities;

namespace BookService.Interface.ServiceInterface
{
    public interface IBookService
    {
        Task<ApiResponse> GetAllAsync();

        Task<ApiResponse> GetByIdAsync(int id);

        Task<ApiResponse> AddAsync(AddBookRequest request);

        Task<ApiResponse> UpdateAsync(UpdateBookRequest request);

        Task<ApiResponse> DeleteAsync(int id);
    }
}
