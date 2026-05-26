using System;

namespace backend_library.Domain.Entities;

public class Loan
{
    public int Id { get; private set; }
    public int BookId { get; private set; }
    public int UserId { get; private set; } // Relación con el Estudiante/Usuario
    public DateTime LoanDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public bool IsExtended { get; private set; }
    public bool IsReturned { get; private set; }

    // Constructor para Entity Framework
    private Loan() { }

    // Constructor de Negocio
    public Loan(int bookId, int userId)
    {
        BookId = bookId;
        UserId = userId;
        LoanDate = DateTime.UtcNow;
        DueDate = DateTime.UtcNow.AddDays(30); // Regla: 30 días iniciales
        IsExtended = false;
        IsReturned = false;
    }

    // Regla de Negocio: Renovar Préstamo
    public void RenewLoan()
    {
        if (IsExtended)
            throw new InvalidOperationException("El préstamo ya ha sido extendido una vez.");
        
        if (IsReturned)
            throw new InvalidOperationException("No se puede extender un préstamo ya devuelto.");

        DueDate = DueDate.AddDays(15); // Regla: 15 días más (Total 45)
        IsExtended = true;
    }

    public void MarkAsReturned()
    {
        IsReturned = true;
    }

    // Regla de Negocio: Verificar si el préstamo está vencido y requiere aplicar sanción
    public bool RequiresSanction()
    {
        // Si ya se devolvió, no hay por qué sancionar
        if (IsReturned) return false;

        // Si la fecha actual superó la fecha límite de devolución
        return DateTime.UtcNow > DueDate;
    }

    public bool RequiresSanction()
{
    // REGLA CRÍTICA: Si ya se devolvió, NUNCA requiere sanción, sin importar la fecha
    if (IsReturned) return false;

    // Si no se ha devuelto y ya venció el plazo, se sanciona
    return DateTime.UtcNow > DueDate;
}
}