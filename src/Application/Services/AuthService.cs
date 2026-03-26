using backend_library.Application.DTOs;
using backend_library.Application.Interfaces;
using backend_library.Domain.Entities;

namespace backend_library.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<string> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByUsernameOrEmailAsync(request.Email) 
                        ?? await _userRepository.GetByUsernameOrEmailAsync(request.Username);

        if (existingUser != null)
            throw new InvalidOperationException("El usuario o correo ya existe.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _userRepository.AddAsync(user);
        return "Usuario registrado exitosamente.";
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(request.UsernameOrEmail);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new InvalidOperationException("Credenciales inválidas.");

        var token = _tokenService.GenerateJwtToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(180);
        
        await _userRepository.UpdateAsync(user);

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(token);
        if (principal == null) 
            throw new InvalidOperationException("Token inválido.");

        var username = principal.Identity?.Name;
        var user = await _userRepository.GetByUsernameAsync(username!);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new InvalidOperationException("Petición de refresh token inválida.");

        var newJwtToken = _tokenService.GenerateJwtToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        await _userRepository.UpdateAsync(user);

        return new AuthResponse
        {
            Token = newJwtToken,
            RefreshToken = newRefreshToken
        };
    }
}