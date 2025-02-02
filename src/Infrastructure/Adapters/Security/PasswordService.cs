using System.Security.Cryptography;
using System.Text;
using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Domain.Interfaces;

namespace MoodTracker_back.Infrastructure.Adapters.Security;

public class PasswordService : IPasswordService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IPasswordResetTokenRepository _resetTokenRepository;
    private readonly ILoggingService _logger;

    public PasswordService(
        IUserRepository userRepository,
        IEmailService emailService,
        ITokenService tokenService,
        IPasswordResetTokenRepository resetTokenRepository,
        ILoggingService logger)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _tokenService = tokenService;
        _resetTokenRepository = resetTokenRepository;
        _logger = logger;
    }
    
    public string HashPassword(string password, out string salt)
    {
        salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        using var sha256 = SHA256.Create();
        var passwordBytes = Encoding.UTF8.GetBytes(password + salt);
        var hashBytes = sha256.ComputeHash(passwordBytes);
        return Convert.ToBase64String(hashBytes);
    }

    public (string Hash, string Salt) SplitPasswordHash(string passwordHash)
    {
        var parts = passwordHash.Split(':');
        return (parts[0], parts[1]);
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

        var expiredTokens = await _resetTokenRepository.GetExpiredTokensAsync();
        if (expiredTokens.Any())
        {
            await _resetTokenRepository.RemoveRangeAsync(expiredTokens);
        }

        var token = _tokenService.GeneratePasswordResetToken(user.Id, user.Email);
        var resetToken = new PasswordResetToken()
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        };

        await _resetTokenRepository.AddAsync(resetToken);
        await _resetTokenRepository.SaveChangesAsync();

        var baseUrl = Environment.GetEnvironmentVariable("WEB_APP_URL");
        var resetLink = $"{baseUrl}/change-password?token={Uri.EscapeDataString(token)}&email={user.Email}";

        return await _emailService.SendPasswordRecoverEmailAsync(user.Email, resetLink);
    }
        
    public async Task<bool> ChangePasswordAsync(string email, string token, string password, string newPassword)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return false;

        var tokenIsValid = _tokenService.ValidatePasswordResetToken(token);
        if (!tokenIsValid.isValid) return false;
        
        var resetToken = await _resetTokenRepository.GetValidTokenAsync(user.Id, token);

        await _resetTokenRepository.MarkAsUsedAsync(resetToken);

        var newSalt = "";
        user.PasswordHash = HashPassword(newPassword, out newSalt);
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _resetTokenRepository.SaveChangesAsync();

        return true;
    }
}