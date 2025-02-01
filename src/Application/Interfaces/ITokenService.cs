using Domain.Entities;

namespace MoodTracker_back.Application.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
    string GeneratePasswordResetToken(int useId, string email);

    (bool isValid, int userId, string email) ValidatePasswordResetToken(string token);
}