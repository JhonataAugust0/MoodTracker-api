using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data.Config;
using Microsoft.EntityFrameworkCore;

namespace MoodTracker_back.Infrastructure.Data.Repositories;

public class QuickNotesRepository : IQuickNoteRepository
{
    private readonly ApplicationDbContext _context;

    public QuickNotesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QuickNote> GetByIdAsync(int id)
    {
        return await _context.QuickNotes.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<QuickNote>> GetByUserIdAsync(int userId)
    {
        return await _context.QuickNotes
            .Include(u => u.User)
            .Where(u => u.UserId == userId)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task CreateAsync(QuickNote note)
    {
        _context.QuickNotes.Add(note);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(QuickNote note)
    {
        _context.Entry(note).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var note = await _context.QuickNotes.FindAsync(id);
        if (note != null)
        {
            _context.QuickNotes.Remove(note);
            await _context.SaveChangesAsync();
        }
    } 
}