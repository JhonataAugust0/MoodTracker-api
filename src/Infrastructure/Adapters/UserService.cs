using Domain.Entities;
using Domain.Interfaces;
using MoodTracker_back.Infrastructure.Exceptions;
using MoodTracker_back.Application.Services;

namespace MoodTracker_back.Infrastructure.Adapters;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;

    public UserService(
        IUserRepository userRepository,
        IPasswordService passwordService
    )
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
    }

    public async Task<User> RegisterUserAsync(string email, string password, string? name)
    {
        if (await _userRepository.EmailExistsAsync(email))
        {
            throw new ValidationException("Email already exists");
        }

        var passwordHash = _passwordService.HashPassword(password, out string salt);
        
        var user = new User()
        {
            Email = email,
            Name = name,
            PasswordHash = passwordHash + ":" + salt
        };

        await _userRepository.CreateAsync(user);
        return user;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }
    
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _userRepository.EmailExistsAsync(email);
    }
    
    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        return await _userRepository.GetUserByRefreshTokenAsync(refreshToken);
    }

    public async Task<IEnumerable<User>> GetInactiveUsers(CancellationToken stoppingToken)
    {
        return await _userRepository.GetInactiveUsers(stoppingToken);
    }
    
    public async Task UpdateUserLastNotifiedAsync(int userId, DateTime lastNotified, CancellationToken stoppingToken)
    {
        await _userRepository.UpdateLastNotifiedAsync(userId, lastNotified, stoppingToken);
    }
    public async Task UpdateUserAsync(User user)
    {
        await _userRepository.UpdateAsync(user);
    }
    
    public async Task DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
            
        await _userRepository.DeleteAsync(user);
    }
}