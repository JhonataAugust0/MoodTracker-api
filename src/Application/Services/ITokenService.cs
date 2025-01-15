using Domain.Entities;

namespace MoodTracker_back.Application.Services;

public interface ITokenService
{
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
    string GeneratePasswordResetToken();
}