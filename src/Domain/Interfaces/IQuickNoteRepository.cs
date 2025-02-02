using MoodTracker_back.Domain.Entities;

namespace MoodTracker_back.Domain.Interfaces;


public interface IQuickNoteRepository
{
    Task<QuickNote> GetByIdAsync(int id);
    Task<IEnumerable<QuickNote>> GetByUserIdAsync(int userId);
    Task CreateAsync(QuickNote quickNoteBase);
    Task UpdateAsync(QuickNote quickNoteBase);
    Task DeleteAsync(int id);
}