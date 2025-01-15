using Domain.Entities;

namespace Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    Task<bool> EmailExistsAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}