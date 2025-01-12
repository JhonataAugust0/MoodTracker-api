using Application.Dtos;
using Domain.Entities;
using Domain.Interfaces;

namespace Infrastructure.Adapters;

using Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IEmailService _emailService;

    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _emailService = emailService;
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string? name)
    {
        if (await _userRepository.EmailExistsAsync(email))
        {
            return new AuthResult { Success = false, Error = "Email already exists" };
        }

        var passwordHash = _passwordHasher.HashPassword(password, out string salt);
        
        var user = new User()
        {
            Email = email,
            Name = name,
            PasswordHash = passwordHash + ":" + salt // Store both hash and salt
        };

        await _userRepository.CreateAsync(user);

        var token = _tokenGenerator.GenerateJwtToken(user);
        var refreshToken = _tokenGenerator.GenerateRefreshToken();

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        });

        await _userRepository.UpdateAsync(user);

        return new AuthResult
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return new AuthResult { Success = false, Error = "Invalid credentials" };
        }

        var parts = user.PasswordHash.Split(':');
        var hash = parts[0];
        var salt = parts[1];

        if (!_passwordHasher.VerifyPassword(password, hash, salt))
        {
            return new AuthResult { Success = false, Error = "Invalid credentials" };
        }

        var token = _tokenGenerator.GenerateJwtToken(user);
        var refreshToken = _tokenGenerator.GenerateRefreshToken();

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        });
        user.LastLogin = DateTimeOffset.UtcNow;

        await _userRepository.UpdateAsync(user);

        return new AuthResult
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userRepository.GetByIdAsync(1); // You'll need to implement a method to find user by refresh token
        if (user == null)
        {
            return new AuthResult { Success = false, Error = "Invalid refresh token" };
        }

        var existingToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);
        if (existingToken == null || existingToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return new AuthResult { Success = false, Error = "Invalid or expired refresh token" };
        }

        var newToken = _tokenGenerator.GenerateJwtToken(user);
        var newRefreshToken = _tokenGenerator.GenerateRefreshToken();

        user.RefreshTokens.Remove(existingToken);
        user.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        });

        await _userRepository.UpdateAsync(user);

        return new AuthResult
        {
            Success = true,
            Token = newToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return false;

        var resetToken = _tokenGenerator.GeneratePasswordResetToken();
        // Store reset token in user record or separate table with expiration

        await _emailService.SendPasswordResetEmailAsync(email, resetToken);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return false;

        // Verify reset token is valid and not expired
        
        var passwordHash = _passwordHasher.HashPassword(newPassword, out string salt);
        user.PasswordHash = passwordHash + ":" + salt;
        
        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var user = await _userRepository.GetByIdAsync(1); // You'll need to implement a method to find user by refresh token
        if (user == null) return false;

        var token = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);
        if (token == null) return false;

        user.RefreshTokens.Remove(token);
        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        var parts = user.PasswordHash.Split(':');
        var hash = parts[0];
        var salt = parts[1];

        if (!_passwordHasher.VerifyPassword(currentPassword, hash, salt))
        {
            return false;
        }

        var passwordHash = _passwordHasher.HashPassword(newPassword, out string newSalt);
        user.PasswordHash = passwordHash + ":" + newSalt;

        await _userRepository.UpdateAsync(user);
        return true;
    }
}