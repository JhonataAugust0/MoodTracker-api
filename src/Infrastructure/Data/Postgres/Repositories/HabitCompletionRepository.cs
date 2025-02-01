using Domain.Entities;
using Domain.Interfaces;
using MoodTracker_back.Infrastructure.Data.Postgres.Config;
using Microsoft.EntityFrameworkCore;

namespace MoodTracker_back.Infrastructure.Data.Postgres.Repositories;


public class HabitCompletionCompletionRepository : IHabitCompletionRepository
{
    private readonly ApplicationDbContext _context;

    public HabitCompletionCompletionRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<HabitCompletion>> GetUserHistoryHabitCompletionAsync(int habitId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        return await _context.HabitCompletions
            .Where(h => h.HabitId == habitId &&
                        (!startDate.HasValue || h.CreatedAt >= startDate.Value) &&
                        (!endDate.HasValue || h.CompletedAt <= endDate.Value.AddDays(1)))
            .OrderByDescending(h => h.CompletedAt)
            .ToListAsync();
    }

    public async Task<HabitCompletion> GetByIdAsync(int id)
    {
        return await _context.HabitCompletions
            .FirstOrDefaultAsync(h => h.Id == id);
    }
    
    public async Task<IEnumerable<HabitCompletion>> GetByHabitIdAsync(int habitId)
    {
        return await _context.HabitCompletions
            .Where(m => m.HabitId == habitId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task CreateAsync(HabitCompletion habit)
    {
         _context.HabitCompletions.Add(habit);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(HabitCompletion habit)
    {
        _context.Entry(habit).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var habit = await  _context.HabitCompletions.FindAsync(id);
        if (habit != null)
        {
             _context.HabitCompletions.Remove(habit);
            await _context.SaveChangesAsync();
        }
    }
}