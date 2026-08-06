using BookService.DTO;
using BookService.Entities;
using BookService.Interface.RepositoryInterface;
using BookService.Interface.ServiceInterface;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BookService.ServiceImplementation
{
    public class BooksService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IDistributedCache _cache;
        public BooksService(IBookRepository bookRepository, IDistributedCache cache)
        {
            _bookRepository = bookRepository;
            _cache = cache;
        }

        public async Task<ApiResponse<List<Book>>> GetAllAsync()
        {
            const string cacheKey = "AllBooks";
            var cachedBooks = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedBooks))
            {
                var booksFromCache = JsonSerializer.Deserialize<List<Book>>(cachedBooks);

                return new ApiResponse<List<Book>>
                {
                    Success = true,
                    Message = "Books fetched from Redis.",
                    Data = booksFromCache
                };
            }

            var books = await _bookRepository.GetAllAsync();
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetAsync(cacheKey, System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(books), options);
            return new ApiResponse<List<Book>>
            {
                Success = true,
                Message = "Books fetched successfully.",
                Data = books
            };
        }

        public async Task<ApiResponse<Book>> GetByIdAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);

            if (book == null)
            {
                return new ApiResponse<Book>
                {
                    Success = false,
                    Message = "Book not found."
                };
            }

            return new ApiResponse<Book>
            {
                Success = true,
                Message = "Book fetched successfully.",
                Data = book
            };
        }

        public async Task<ApiResponse<string>> AddAsync(AddBookRequest request)
        {
            var book = new Book
            {
                Title = request.Title,
                Author = request.Author,
                Price = request.Price,
                Stock = request.Stock
            };

            await _bookRepository.AddAsync(book);
            await _bookRepository.SaveChangesAsync();

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Book added successfully."
            };
        }

        public async Task<ApiResponse<string>> UpdateAsync(UpdateBookRequest request)
        {
            var book = await _bookRepository.GetByIdAsync(request.Id);

            if (book == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Book not found."
                };
            }

            book.Title = request.Title;
            book.Author = request.Author;
            book.Price = request.Price;
            book.Stock = request.Stock;

            await _bookRepository.UpdateAsync(book);
            await _bookRepository.SaveChangesAsync();

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Book updated successfully."
            };
        }

        public async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);

            if (book == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Book not found."
                };
            }

            await _bookRepository.DeleteAsync(book);
            await _bookRepository.SaveChangesAsync();

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Book deleted successfully."
            };
        }
    }
}
