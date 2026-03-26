namespace backend_library.Application.DTOs;

public class LoginRequest
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}