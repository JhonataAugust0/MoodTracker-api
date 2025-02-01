using Domain.Entities;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Infrastructure.Data.Postgres.Config;
using Microsoft.EntityFrameworkCore;

namespace MoodTracker_back.Infrastructure.Data.Postgres.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly ApplicationDbContext _context;

    public PasswordResetTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PasswordResetToken?> GetValidTokenAsync(int userId, string token)
    {
        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => 
                t.UserId == userId && 
                t.Token == token && 
                !t.Used && 
                t.ExpiresAt > DateTimeOffset.UtcNow);
    }

    public async Task<IEnumerable<PasswordResetToken>> GetExpiredTokensAsync()
    {
        return await _context.PasswordResetTokens
            .Where(t => !t.Used && t.ExpiresAt < DateTimeOffset.UtcNow)
            .ToListAsync();
    }

    public async Task AddAsync(PasswordResetToken token)
    {
        await _context.PasswordResetTokens.AddAsync(token);
    }

    public async Task MarkAsUsedAsync(PasswordResetToken token)
    {
        token.Used = true;
        _context.PasswordResetTokens.Update(token);
    }

    public async Task RemoveRangeAsync(IEnumerable<PasswordResetToken> tokens)
    {
        _context.PasswordResetTokens.RemoveRange(tokens);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}