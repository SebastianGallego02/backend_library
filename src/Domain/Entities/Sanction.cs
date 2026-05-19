using System;

namespace backend_library.Domain.Entities;

public class Sanction
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    private Sanction() { }

    public Sanction(int userId)
    {
        UserId = userId;
        StartDate = DateTime.UtcNow;
        EndDate = DateTime.UtcNow.AddMonths(2); // Regla del flujo: 2 meses de sanción
    }

    // Propiedad para validar si la sanción sigue vigente hoy
    public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
}