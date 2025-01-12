using Domain.Entities;

namespace Application.Services;

public interface ITokenGenerator
{
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
    string GeneratePasswordResetToken();
}