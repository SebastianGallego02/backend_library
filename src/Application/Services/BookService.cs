using backend_library.Application.DTOs;
using backend_library.Application.Interfaces;
using backend_library.Domain.Entities;

namespace backend_library.Application.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<IEnumerable<BookResponseDto>> GetAllBooksAsync()
    {
        var books = await _bookRepository.GetAllAsync();
        return books.Select(b => new BookResponseDto(b.Id, b.Title, b.Author, b.Description, b.PublishedYear, b.TotalCopies));
    }

    public async Task<BookResponseDto?> GetBookByIdAsync(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null) return null;
        return new BookResponseDto(book.Id, book.Title, book.Author, book.Description, book.PublishedYear, book.TotalCopies);
    }

    public async Task<BookResponseDto> CreateBookAsync(BookCreateUpdateDto dto)
    {
        if (dto.TotalCopies <= 0)
        {
            throw new ArgumentException("El número de copias debe ser mayor a cero.");
        }

        var book = new Book { Title = dto.Title, Author = dto.Author, Description = dto.Description, PublishedYear = dto.PublishedYear };
        book.UpdateStock(dto.TotalCopies);
        await _bookRepository.AddAsync(book);
        await _bookRepository.SaveChangesAsync();
        return new BookResponseDto(book.Id, book.Title, book.Author, book.Description, book.PublishedYear, book.TotalCopies);
    }

    public async Task<bool> UpdateBookAsync(int id, BookCreateUpdateDto dto)
    {
        if (dto.TotalCopies <= 0)
        {
            throw new ArgumentException("El número de copias debe ser mayor a cero.");
        }

        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null) return false;

        book.Title = dto.Title;
        book.Author = dto.Author;
        book.Description = dto.Description;
        book.PublishedYear = dto.PublishedYear;
        book.UpdateStock(dto.TotalCopies);

        _bookRepository.Update(book);
        return await _bookRepository.SaveChangesAsync();
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null) return false;

        _bookRepository.Delete(book);
        return await _bookRepository.SaveChangesAsync();
    }
}