using Domain.Entities;
using MoodTracker_back.Application.Dtos;
using MoodTracker_back.Application.Interfaces;


namespace MoodTracker_back.Application.Services;

public class AuthenticationAppService : IAuthenticationService
{
    private readonly IUserService _userService;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenGenerator;
    private readonly ILoggingService _logger;

    public AuthenticationAppService(
        IUserService userService,
        IPasswordService PasswordService,
        ITokenService tokenGenerator,
        ILoggingService logger)
    {
        _userService = userService;
        _passwordService = PasswordService;
        _tokenGenerator = tokenGenerator;
        _logger = logger;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        await _logger.LogInformationAsync("Usuário de email " + email + " realizando login", [email]);
        var user = await _userService.GetUserByEmailAsync(email);
        if (user == null)
        {
            return new AuthResult { Success = false, Error = "Invalid credentials" };
        }

        var (hash, salt) = _passwordService.SplitPasswordHash(user.PasswordHash);
        if (!_passwordService.VerifyPassword(password, hash, salt))
        {
            return new AuthResult { Success = false, Error = "Credenciais inválidas" };
        }


        var token = _tokenGenerator.GenerateJwtToken(user);
        var refreshToken = _tokenGenerator.GenerateRefreshToken();

        user.RefreshTokens.Add(new RefreshToken()
        {
            Token = refreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        });
        user.LastLogin = DateTimeOffset.UtcNow;

        await _userService.UpdateUserAsync(user);
        await _logger.LogInformationAsync("Login do usuário de email " + email + " bem sucedido", [email]);
        
        return new AuthResult
        {
            Success = true,
            Token = "Bearer " + token,
            RefreshToken = refreshToken,
            UserId = user.Id,
            UserEmail = user.Email
        };
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        await _logger.LogInformationAsync("Usuário de refreshToken " + refreshToken + " realizando renovação da sessão", [refreshToken]);
        var user = await _userService.GetUserByRefreshTokenAsync(refreshToken);
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
    
        await _logger.LogInformationAsync("Renovação da sessão do usuário de refreshToken " + refreshToken + " bem sucedida", [refreshToken]);
        await _userService.UpdateUserAsync(user);

        return new AuthResult
        {
            Success = true,
            Token = newToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var user = await _userService.GetUserByRefreshTokenAsync(refreshToken);
        if (user == null) return false;

        var token = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);
        if (token == null) return false;
    
        await _logger.LogInformationAsync("Usuário de refreshToken " + refreshToken + " realizando logout", [refreshToken]);
        user.RefreshTokens.Remove(token);
        await _userService.UpdateUserAsync(user);
        return true;
    }
}