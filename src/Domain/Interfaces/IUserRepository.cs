using MoodTracker_back.Domain.Entities;

namespace MoodTracker_back.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    Task<bool> EmailExistsAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetInactiveUsers(CancellationToken stoppingToken);
    Task UpdateLastNotifiedAsync(int userId, DateTime lastNotified, CancellationToken stoppingToken);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
}