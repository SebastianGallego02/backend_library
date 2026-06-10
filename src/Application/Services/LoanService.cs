using System;
using System.Collections.Generic;
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

    public async Task<IEnumerable<LoanResponseDto>> GetLoansAsync(Guid? userId = null)
    {
        var loansQuery = _context.Loans.AsQueryable();

        if (userId.HasValue)
        {
            loansQuery = loansQuery.Where(l => l.UserId == userId.Value);
        }

        var loans = await loansQuery.ToListAsync();

        return loans.Select(loan => new LoanResponseDto(
            loan.Id,
            loan.BookId,
            loan.UserId,
            loan.DueDate.ToString("yyyy-MM-dd"),
            loan.ReturnDate?.ToString("yyyy-MM-dd"),
            loan.IsExtended,
            loan.IsReturned
        ));
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
        // Si existieron registros antiguos con TotalCopies incorrecto, los corregimos antes de evaluar.
        if (book.TotalCopies <= 0)
        {
            Console.WriteLine($"[WARNING] Libro '{book.Title}' (ID: {book.Id}) tenía TotalCopias inválido ({book.TotalCopies}). Se corrige a 1 copia.");
            book.UpdateStock(1);
        }
        // Actualizamos el estado del libro basándonos en la base de datos real justo antes de evaluar
        book.UpdateAvailability(activeLoansCount);
        if (!book.IsAvailable)
        {
            Console.WriteLine($"[DEBUG] Intento de préstamo para el libro '{book.Title}' (ID: {book.Id}) que está agotado. TotalCopias: {book.TotalCopies}, ActiveLoans: {activeLoansCount}");
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
            newLoan.ReturnDate?.ToString("yyyy-MM-dd"),
            newLoan.IsExtended,
            newLoan.IsReturned
        );
    }

    public async Task ProcessExpiredLoansAsync()
    {
        // Filtrado estricto: Solo préstamos que NO se han devuelto Y que ya expiraron
        var expiredLoans = await _context.Loans
            .Where(l => !l.IsReturned && DateTime.UtcNow > l.DueDate)
            .ToListAsync();

        foreach (var loan in expiredLoans)
        {
            // Verificar si ya tiene una sanción vigente para no duplicarla
            var alreadySanctioned = await _context.Sanctions
                .AnyAsync(s => s.UserId == loan.UserId && DateTime.UtcNow <= s.EndDate);

            if (!alreadySanctioned)
            {
                var newSanction = new Sanction(loan.UserId);
                _context.Sanctions.Add(newSanction);
            }
        }

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
            loan.ReturnDate?.ToString("yyyy-MM-dd"),
            loan.IsExtended,
            loan.IsReturned
        );
    }

    public async Task<LoanResponseDto> ReturnLoanAsync(ReturnLoanRequestDto request)
    {
        var loan = await _context.Loans.FindAsync(request.LoanId);
        if (loan == null) throw new ArgumentException("El préstamo no existe.");
        if (loan.IsReturned) throw new InvalidOperationException("Este préstamo ya fue devuelto.");

        // Si está devolviendo el libro pero YA se pasó de la fecha límite
        if (DateTime.UtcNow > loan.DueDate)
        {
            // Verificar si ya tiene sanción para no duplicar
            var alreadySanctioned = await _context.Sanctions
                .AnyAsync(s => s.UserId == loan.UserId && DateTime.UtcNow <= s.EndDate);

            if (!alreadySanctioned)
            {
                // Se le aplica la sanción de 2 meses de inmediato por devolución tardía
                var instantSanction = new Sanction(loan.UserId);
                _context.Sanctions.Add(instantSanction);
            }
        }

        // Continuamos con el flujo normal de entrega
        loan.MarkAsReturned();

        var book = await _context.Books.FindAsync(loan.BookId);
        if (book != null)
        {
            var activeLoansCount = await _context.Loans
                .CountAsync(l => l.BookId == loan.BookId && !l.IsReturned && l.Id != loan.Id);
            book.UpdateAvailability(activeLoansCount);
        }

        await _context.SaveChangesAsync();

        // ¡AQUÍ ESTÁ EL RETURN QUE FALTABA!
        return new LoanResponseDto(
            loan.Id,
            loan.BookId,
            loan.UserId,
            loan.DueDate.ToString("yyyy-MM-dd"),
            loan.ReturnDate?.ToString("yyyy-MM-dd"),
            loan.IsExtended,
            loan.IsReturned
        );
    }
}