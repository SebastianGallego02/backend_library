using System;
using System.Linq;
using System.Threading.Tasks;
using backend_library.Application.DTOs;
using backend_library.Domain.Entities;
using backend_library.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend_library.Application.Services;

public class LoanService : ILoanService
{
    private readonly AppDbContext _context;

    public LoanService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LoanResponseDto> CreateLoanAsync(CreateLoanRequestDto request)
    {
        // 1. REGLA: Validar si el estudiante está sancionado
        var hasActiveSanction = await _context.Sanctions
            .AnyAsync(s => s.UserId == request.UserId && DateTime.UtcNow >= s.StartDate && DateTime.UtcNow <= s.EndDate);

        if (hasActiveSanction)
        {
            throw new InvalidOperationException("El estudiante no está habilitado para realizar préstamos debido a una sanción activa.");
        }

        // 2. REGLA: Validar stock de copias del libro utilizando el nuevo flag del Dominio
        var book = await _context.Books.FindAsync(request.BookId);
        if (book == null)
        {
            throw new ArgumentException("El libro solicitado no existe.");
        }

        // Contar cuántos préstamos activos tiene ese libro en este momento
        var activeLoansCount = await _context.Loans
            .CountAsync(l => l.BookId == request.BookId && !l.IsReturned);

        // Actualizamos el estado del libro basándonos en la base de datos real justo antes de evaluar
        book.UpdateAvailability(activeLoansCount);

        if (!book.IsAvailable)
        {
            throw new InvalidOperationException("El libro se encuentra agotado actualmente (no hay más copias disponibles).");
        }

        // 3. Crear el préstamo si pasa los filtros de las imágenes
        var newLoan = new Loan(request.BookId, request.UserId);

        _context.Loans.Add(newLoan);
        await _context.SaveChangesAsync();

        return new LoanResponseDto(
            newLoan.Id, 
            newLoan.BookId, 
            newLoan.UserId, 
            newLoan.DueDate.ToString("yyyy-MM-dd"), 
            newLoan.IsExtended
        );
    }

    public async Task ProcessExpiredLoansAsync()
    {
    // 1. Buscar todos los préstamos que ya vencieron y no han sido devueltos
    var expiredLoans = await _context.Loans
        .Where(l => !l.IsReturned && DateTime.UtcNow > l.DueDate)
        .ToListAsync();

    foreach (var loan in expiredLoans)
    {
        // 2. Verificar si el usuario ya tiene una sanción activa para no duplicarla
        var alreadySanctioned = await _context.Sanctions
            .AnyAsync(s => s.UserId == loan.UserId && DateTime.UtcNow <= s.EndDate);

        if (!alreadySanctioned)
        {
            // 3. Si no está sancionado, le creamos la sanción automática de 2 meses (Regla del flujo)
            var newSanction = new Sanction(loan.UserId);
            _context.Sanctions.Add(newSanction);
        }
    }

    // Guardar todos los cambios en la base de datos de Docker
    await _context.SaveChangesAsync();
    }

    public async Task<LoanResponseDto> RenewLoanAsync(int loanId)
{
    var loan = await _context.Loans.FindAsync(loanId);
    if (loan == null)
    {
        throw new ArgumentException("El préstamo no existe.");
    }

    // Ejecuta la regla del dominio (valida si ya se extendió o si ya se devolvió)
    loan.RenewLoan();

    await _context.SaveChangesAsync();

    return new LoanResponseDto(
        loan.Id,
        loan.BookId,
        loan.UserId,
        loan.DueDate.ToString("yyyy-MM-dd"),
        loan.IsExtended
    );
}
}