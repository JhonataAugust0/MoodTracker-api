using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data.Config;
using Microsoft.EntityFrameworkCore;

namespace MoodTracker_back.Infrastructure.Data.Repositories;


public class MoodRepository : IMoodRepository
{
    private readonly ApplicationDbContext _context;

    public MoodRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Mood> GetByIdAsync(int id)
    {
        return await _context.MoodEntries
            .Include(m => m.Tags)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
    
    public async Task<IEnumerable<Mood>> GetUserMoodsAsync(int userId)
    {
        return await _context.MoodEntries
            .Include(h => h.Tags)
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mood>> GetUserHistoryMoodAsync(int moodId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        return await _context.MoodEntries
            .Include(m => m.Tags)
            .Where(m => m.Id == moodId &&
                        (!startDate.HasValue || m.Timestamp >= startDate.Value) &&
                        (!endDate.HasValue || m.Timestamp <= endDate.Value.AddDays(1)))
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mood>> GetByUserIdAsync(int userId)
    {
        return await _context.MoodEntries
            .Include(m => m.Tags)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task CreateAsync(Mood mood)
    {
        _context.MoodEntries.Add(mood);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Mood mood)
    {
        _context.Entry(mood).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var mood = await _context.MoodEntries.FindAsync(id);
        if (mood != null)
        {
            _context.MoodEntries.Remove(mood);
            await _context.SaveChangesAsync();
        }
    }
}