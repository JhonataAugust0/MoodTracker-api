using Domain.Entities;

namespace MoodTracker_back.Application.Services;

public interface IUserService
{
    Task<User> RegisterUserAsync(string email, string password, string? name);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    Task<bool> EmailExistsAsync(string email);
    Task UpdateUserAsync(User user);
}