using System.Collections.Generic;
using System.Threading.Tasks;
using backend_library.Application.DTOs;

namespace backend_library.Application.Interfaces;

public interface ISanctionService
{
    Task<IEnumerable<SanctionResponseDto>> GetActiveSanctionsAsync();
    Task<IEnumerable<SanctionResponseDto>> GetSanctionHistoryAsync();
    Task<SanctionResponseDto> DeactivateSanctionAsync(int id);
    Task<SanctionResponseDto> ExtendSanctionAsync(int id, int months);
}