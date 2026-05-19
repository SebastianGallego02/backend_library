namespace backend_library.Application.DTOs;

public record CreateLoanRequestDto(int BookId, int UserId);

public record LoanResponseDto(int Id, int BookId, int UserId, string DueDate, bool IsExtended);