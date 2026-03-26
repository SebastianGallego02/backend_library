using System.Security.Claims;
using backend_library.Domain.Entities;

namespace backend_library.Application.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}