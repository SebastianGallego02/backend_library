using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_library.Application.DTOs;
using backend_library.Application.Interfaces;

namespace backend_library.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // 🔒 Bloquea todo el controlador. Requiere cualquier usuario autenticado de base.
public class BookController : ControllerBase
{
    private readonly IBookService _bookService;

    public BookController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet] // 🔓 Accesible por cualquier rol autenticado
    public async Task<IActionResult> GetAll()
    {
        var books = await _bookService.GetAllBooksAsync();
        return Ok(books);
    }

    [HttpGet("{id}")] // 🔓 Accesible por cualquier rol autenticado
    public async Task<IActionResult> GetById(int id)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        if (book == null) return NotFound(new { message = "Libro no encontrado" });
        return Ok(book);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")] // 🛑 RESTRICCIÓN: Solo usuarios con el Claim de Rol "Admin"
    public async Task<IActionResult> Create([FromBody] BookCreateUpdateDto dto)
    {
        var newBook = await _bookService.CreateBookAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = newBook.Id }, newBook);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")] // 🛑 RESTRICCIÓN: Solo Admin
    public async Task<IActionResult> Update(int id, [FromBody] BookCreateUpdateDto dto)
    {
        var updated = await _bookService.UpdateBookAsync(id, dto);
        if (!updated) return NotFound(new { message = "Libro no encontrado o no se pudo actualizar" });
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // 🛑 RESTRICCIÓN: Solo Admin
    public async Task<IActionResult> Delete(int id)

    {
        var deleted = await _bookService.DeleteBookAsync(id);
        if (!deleted) return NotFound(new { message = "Libro no encontrado o no se pudo eliminar" });
        return NoContent();
    }
}