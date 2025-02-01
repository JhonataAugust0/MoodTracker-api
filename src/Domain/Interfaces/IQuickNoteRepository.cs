using Domain.Entities;

namespace MoodTracker_back.Domain.Interfaces;


public interface IQuickNoteRepository
{
    Task<QuickNote> GetByIdAsync(int id);
    Task<IEnumerable<QuickNote>> GetByUserIdAsync(int userId);
    Task CreateAsync(QuickNote quickNote);
    Task UpdateAsync(QuickNote quickNote);
    Task DeleteAsync(int id);
}