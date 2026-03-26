using backend_library.Domain.Entities;

namespace backend_library.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}