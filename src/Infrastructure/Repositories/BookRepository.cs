using Microsoft.EntityFrameworkCore;
using backend_library.Application.Interfaces;
using backend_library.Domain.Entities;
using backend_library.Infrastructure.Data;

namespace backend_library.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;

    public BookRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> GetAllAsync() => 
        await _context.Books.ToListAsync();

    public async Task<Book?> GetByIdAsync(int id) => 
        await _context.Books.FindAsync(id);

    public async Task AddAsync(Book book) => 
        await _context.Books.AddAsync(book);

    public void Update(Book book) => 
        _context.Books.Update(book);

    public void Delete(Book book) => 
        _context.Books.Remove(book);

    public async Task<bool> SaveChangesAsync() => 
        await _context.SaveChangesAsync() > 0;
}