namespace backend_library.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Username { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string Role { get; set; } = "User";

    public string PasswordHash { get; set; } = string.Empty;
    
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiryTime { get; set; }
}