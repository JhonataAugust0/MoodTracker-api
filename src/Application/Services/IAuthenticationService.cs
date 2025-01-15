using Application.Dtos;

namespace MoodTracker_back.Application.Services;

public interface IAuthenticationService
{
    Task<AuthResult> RegisterAsync(string email, string password, string? name);
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
}
