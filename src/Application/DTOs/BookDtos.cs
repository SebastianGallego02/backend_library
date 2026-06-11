using System.ComponentModel.DataAnnotations;

namespace backend_library.Application.DTOs;

public record BookCreateUpdateDto(
    string Title,
    string Author,
    string? Description,
    int PublishedYear,
    [Range(1, int.MaxValue, ErrorMessage = "El número de copias debe ser mayor a cero.")] int TotalCopies,
    string? ImageUrl
);

public record BookResponseDto(
    int Id,
    string Title,
    string Author,
    string? Description,
    int PublishedYear,
    int TotalCopies,
    string? ImageUrl
);