using System;

namespace backend_library.Application.DTOs;

public record SanctionResponseDto(
    int Id, 
    Guid UserId, 
    DateTime StartDate, 
    DateTime EndDate, 
    bool IsActive
);

public record ExtendSanctionRequest(int Months);