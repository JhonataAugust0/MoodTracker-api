using MoodTracker_back.Domain.Entities;

namespace MoodTracker_back.Domain.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetValidTokenAsync(int userId, string token);
    Task<IEnumerable<PasswordResetToken>> GetExpiredTokensAsync();
    Task AddAsync(PasswordResetToken token);
    Task MarkAsUsedAsync(PasswordResetToken token);
    Task RemoveRangeAsync(IEnumerable<PasswordResetToken> tokens);
    Task SaveChangesAsync();
}