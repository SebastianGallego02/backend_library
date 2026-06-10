using System;

namespace backend_library.Domain.Entities;

public class Sanction
{
    public int Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }

    private Sanction() { }

    public Sanction(Guid userId)
    {
        UserId = userId;
        StartDate = DateTime.UtcNow;
        EndDate = DateTime.UtcNow.AddMonths(2);
        IsActive = true;
    }

    // Métodos para modificar el estado (DDD/Encapsulamiento)
    public void Deactivate() => IsActive = false;
    public void Extend(int months) => EndDate = EndDate.AddMonths(months);
}