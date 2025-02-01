using Domain.Entities;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Infrastructure.Data.Postgres.Config;
using Microsoft.EntityFrameworkCore;

namespace MoodTracker_back.Infrastructure.Data.Postgres.Repositories;

public class TagRepository : ITagRepository
{
    private readonly ApplicationDbContext _context;

    public TagRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Tag> GetByIdAsync(int id)
    {
        return await _context.Tags.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Tag>> GetByUserIdAsync(int userId)
    {
        return await _context.Tags
            .Include(u => u.User)
            .Where(u => u.UserId == userId)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Tag>> GetUserTagsAsync(int userId)
    {
        return await _context.Tags
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<int> ids)
    {
        if (ids == null || !ids.Any())
            return Enumerable.Empty<Tag>();

        return await _context.Tags
            .Where(tag => ids.Contains(tag.Id))
            .ToListAsync();
    }

    public async Task CreateAsync(Tag tag)
    {
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Tag tag)
    {
        _context.Entry(tag).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag != null)
        {
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }
    } 
}