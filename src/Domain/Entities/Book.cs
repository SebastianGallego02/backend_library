namespace backend_library.Domain.Entities;

public class Book
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public string? Description { get; set; }
    public int PublishedYear { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}