using Domain.Entities;

namespace MoodTracker_back.Domain.Interfaces;


public interface IMoodRepository
{
    Task<Mood> GetByIdAsync(int id);
    Task<IEnumerable<Mood>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Mood>> GetUserMoodsAsync(int userId);
    Task<IEnumerable<Mood>> GetUserHistoryMoodAsync(int moodId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    Task CreateAsync(Mood mood);
    Task UpdateAsync(Mood mood);
    Task DeleteAsync(int id);
}