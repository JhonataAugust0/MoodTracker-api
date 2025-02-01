using Domain.Entities;
using Domain.Interfaces;
using MoodTracker_back.Infrastructure.Data.Postgres.Config;
using Microsoft.EntityFrameworkCore;

namespace MoodTracker_back.Infrastructure.Data.Postgres.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
    
    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken));
    }

    public async Task<IEnumerable<User>> GetInactiveUsers(CancellationToken stoppingToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-3);
        return await _context.Users
            .Where(u => u.LastMoodEntry < cutoff &&
                        (!u.LastNotified.HasValue || u.LastNotified < cutoff))
            .ToListAsync(stoppingToken);
    }

    public async Task UpdateLastNotifiedAsync(int userId, DateTime lastNotified, CancellationToken stoppingToken)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, stoppingToken);
        if (user != null)
        {
            user.LastNotified = lastNotified;
            await _context.SaveChangesAsync(stoppingToken);
        }
    }
    
    public async Task CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }
    
    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }
}