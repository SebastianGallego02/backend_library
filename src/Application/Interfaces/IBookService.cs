using backend_library.Application.DTOs;

namespace backend_library.Application.Interfaces;

public interface IBookService
{
    Task<IEnumerable<BookResponseDto>> GetAllBooksAsync();
    Task<BookResponseDto?> GetBookByIdAsync(int id);
    Task<BookResponseDto> CreateBookAsync(BookCreateUpdateDto dto);
    Task<bool> UpdateBookAsync(int id, BookCreateUpdateDto dto);
    Task<bool> DeleteBookAsync(int id);
}