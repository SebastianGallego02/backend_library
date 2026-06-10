using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend_library.Application.DTOs;

namespace backend_library.Application.Services;

public interface ILoanService
{
    Task<IEnumerable<LoanResponseDto>> GetLoansAsync(Guid? userId = null);
    Task<LoanResponseDto> CreateLoanAsync(CreateLoanRequestDto request);
    Task ProcessExpiredLoansAsync();
    Task<LoanResponseDto> RenewLoanAsync(int loanId);
    Task<LoanResponseDto> ReturnLoanAsync(ReturnLoanRequestDto request);
}