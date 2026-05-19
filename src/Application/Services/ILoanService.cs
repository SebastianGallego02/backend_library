using System.Threading.Tasks;
using backend_library.Application.DTOs;

namespace backend_library.Application.Services;

public interface ILoanService
{
    Task<LoanResponseDto> CreateLoanAsync(CreateLoanRequestDto request);
    Task ProcessExpiredLoansAsync();
    Task<LoanResponseDto> RenewLoanAsync(int loanId);
}