using System.Security.Cryptography;
using System.Text;
using Domain.Interfaces;
using MoodTracker_back.Application.Services;

namespace MoodTracker_back.Infrastructure.Adapters;

public class PasswordService : IPasswordService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public PasswordService(
        IUserRepository userRepository,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }
    
    public string HashPassword(string password, out string salt)
    {
        salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        using var sha256 = SHA256.Create();
        var passwordBytes = Encoding.UTF8.GetBytes(password + salt);
        var hashBytes = sha256.ComputeHash(passwordBytes);
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        using var sha256 = SHA256.Create();
        var passwordBytes = Encoding.UTF8.GetBytes(password + salt);
        var hashBytes = sha256.ComputeHash(passwordBytes);
        var computedHash = Convert.ToBase64String(hashBytes);
        return hash == computedHash;
    }
    
    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return false;

        var resetToken = Guid.NewGuid().ToString();
        await _emailService.SendPasswordResetEmailAsync(email, resetToken);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return false;
        
        var passwordHash = HashPassword(newPassword, out string salt);
        user.PasswordHash = passwordHash + ":" + salt;
        
        await _userRepository.UpdateAsync(user);
        return true;
    }
    
    public async Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return false;

        var parts = user.PasswordHash.Split(':');
        var hash = parts[0];
        var salt = parts[1];

        if (!VerifyPassword(currentPassword, hash, salt))
        {
            return false;
        }

        var passwordHash = HashPassword(newPassword, out string newSalt);
        user.PasswordHash = passwordHash + ":" + newSalt;

        await _userRepository.UpdateAsync(user);
        return true;
    }
}