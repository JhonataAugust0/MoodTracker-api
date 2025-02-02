using MoodTracker_back.Domain.Entities;

namespace MoodTracker_back.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken> GetByIdAsync(int id);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(int userId);
    Task CreateAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task DeleteAsync(int id);
}