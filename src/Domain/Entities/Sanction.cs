using System;

namespace backend_library.Domain.Entities;

public class Sanction
{
    public int Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    private Sanction() { }

    public Sanction(Guid userId)
    {
        UserId = userId;
        StartDate = DateTime.UtcNow;
        EndDate = DateTime.UtcNow.AddMonths(2); 
    }

    // Propiedad para validar si la sanción sigue vigente hoy
    public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
}