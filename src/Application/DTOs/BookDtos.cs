namespace backend_library.Application.DTOs;

public record BookCreateUpdateDto(
    string Title,
    string Author,
    string? Description,
    int PublishedYear
);

public record BookResponseDto(
    int Id,
    string Title,
    string Author,
    string? Description,
    int PublishedYear
);