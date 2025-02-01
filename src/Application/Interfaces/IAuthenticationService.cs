
using MoodTracker_back.Application.Dtos;

namespace MoodTracker_back.Application.Interfaces;

public interface IAuthenticationService
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
}
