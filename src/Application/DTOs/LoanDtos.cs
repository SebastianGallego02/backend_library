namespace backend_library.Application.DTOs;

// 1. Cambiamos int por Guid en el UserId
public record CreateLoanRequestDto(int BookId, Guid UserId);

// 2. Lo mismo para la respuesta si lo necesitas
public record LoanResponseDto(int Id, int BookId, Guid UserId, string DueDate, string? ReturnDate, bool IsExtended, bool IsReturned);

public record ReturnLoanRequestDto(int LoanId);