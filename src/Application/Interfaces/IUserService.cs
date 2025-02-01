using Domain.Entities;

namespace MoodTracker_back.Application.Interfaces;

public interface IUserService
{
    Task<User> RegisterUserAsync(string email, string password, string? name);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    Task<bool> EmailExistsAsync(string email);
    Task<IEnumerable<User>> GetInactiveUsers(CancellationToken stoppingToken);
    Task UpdateUserLastNotifiedAsync(int userId, DateTime lastNotified, CancellationToken stoppingToken);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
}