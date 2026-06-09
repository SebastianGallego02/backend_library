using System;

namespace backend_library.Domain.Entities;

public class Book
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public string? Description { get; set; }
    public int PublishedYear { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // 📁 Nuevas propiedades para el control de inventario
    public int TotalCopies { get; private set; } = 1; // Copias totales que posee la biblioteca
    public bool IsAvailable { get; private set; } = true; // Flag de disponibilidad (Agotado/Disponible)

    // Constructor requerido por Entity Framework
    public Book() { }

    // Método de negocio para inicializar o aumentar el inventario de este libro
    public void UpdateStock(int totalCopies)
    {
        if (totalCopies < 0)
            throw new ArgumentException("El número de copias no puede ser negativo.");

        TotalCopies = totalCopies;
        UpdateAvailability(0); // Inicializa el estado de disponibilidad
    }

    // Método interno para evaluar si el libro se marcó como agotado
    public void UpdateAvailability(int activeLoansCount)
    {
        // Si los préstamos activos igualan o superan las copias totales, se marca como agotado (IsAvailable = false)
        IsAvailable = activeLoansCount < TotalCopies;
    }
}