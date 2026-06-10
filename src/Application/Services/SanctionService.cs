using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend_library.Application.DTOs;
using backend_library.Application.Interfaces;
using backend_library.Infrastructure.Data;

namespace backend_library.Application.Services;

public class SanctionService : ISanctionService
{
    private readonly AppDbContext _context;

    public SanctionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SanctionResponseDto>> GetActiveSanctionsAsync()
    {
        var now = DateTime.UtcNow;
        var sanctions = await _context.Sanctions
            .Where(s => s.IsActive && s.EndDate >= now)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();

        return sanctions.Select(MapToDto);
    }

    public async Task<IEnumerable<SanctionResponseDto>> GetSanctionHistoryAsync()
    {
        var now = DateTime.UtcNow;
        var sanctions = await _context.Sanctions
            .Where(s => s.EndDate < now || !s.IsActive)
            .OrderByDescending(s => s.EndDate)
            .ToListAsync();

        return sanctions.Select(MapToDto);
    }

    public async Task<SanctionResponseDto> DeactivateSanctionAsync(int id)
    {
        var sanction = await _context.Sanctions.FindAsync(id);
        if (sanction == null) throw new KeyNotFoundException("Sanción no encontrada");

        if (!sanction.IsActive) throw new InvalidOperationException("La sanción ya no está activa");

        sanction.Deactivate();
        await _context.SaveChangesAsync();
        return MapToDto(sanction);
    }

    public async Task<SanctionResponseDto> ExtendSanctionAsync(int id, int months)
    {
        if (months <= 0) throw new ArgumentException("Los meses deben ser mayores a cero");

        var sanction = await _context.Sanctions.FindAsync(id);
        if (sanction == null) throw new KeyNotFoundException("Sanción no encontrada");

        if (!sanction.IsActive) throw new InvalidOperationException("No se puede extender una sanción inactiva");

        if (sanction.EndDate < DateTime.UtcNow)
            throw new InvalidOperationException("No se puede extender una sanción vencida");

        sanction.Extend(months);
        await _context.SaveChangesAsync();
        return MapToDto(sanction);
    }

    private static SanctionResponseDto MapToDto(Domain.Entities.Sanction s) =>
        new SanctionResponseDto(
            s.Id,
            s.UserId,
            s.StartDate,
            s.EndDate,
            s.IsActive
        );
}