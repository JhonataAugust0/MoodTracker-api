using MoodTracker_back.Domain.Entities;

namespace MoodTracker_back.Domain.Interfaces;


public interface IHabitRepository
{
    Task<Habit> GetByIdAsync(int id);
    Task<IEnumerable<Habit>> GetUserHabitsAsync(int userId);
    Task CreateAsync(Habit habitBase);
    Task UpdateAsync(Habit habitBase);
    Task DeleteAsync(int id);
}