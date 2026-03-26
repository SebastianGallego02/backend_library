using backend_library.Application.DTOs;

namespace backend_library.Application.Services;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken);
}