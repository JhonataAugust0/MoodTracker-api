using Domain.Entities;
using Domain.Interfaces;
using MoodTracker_back.Infrastructure.Data.Postgres.Config;
using Microsoft.EntityFrameworkCore;

namespace MoodTracker_back.Infrastructure.Data.Postgres.Repositories;


public class HabitRepository : IHabitRepository
{
    private readonly ApplicationDbContext _context;

    public HabitRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Habit> GetByIdAsync(int id)
    {
        return await _context.Habits
            .Include(m => m.Tags)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Habit>> GetUserHabitsAsync(int userId)
    {
        return await _context.Habits
            .Include(h => h.Tags)
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Habit>> GetByUserIdAsync(int userId)
    {
        return await _context.Habits
            .Include(m => m.Tags)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task CreateAsync(Habit habit)
    {
        _context.Habits.Add(habit);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Habit habit)
    {
        _context.Entry(habit).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var habit = await _context.Habits.FindAsync(id);
        if (habit != null)
        {
            _context.Habits.Remove(habit);
            await _context.SaveChangesAsync();
        }
    }
}