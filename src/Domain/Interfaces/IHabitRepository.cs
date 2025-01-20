using Domain.Entities;

namespace Domain.Interfaces;


public interface IHabitRepository
{
    Task<Habit> GetByIdAsync(int id);
    Task<IEnumerable<Habit>> GetUserHabitsAsync(int userId);
    Task CreateAsync(Habit habit);
    Task UpdateAsync(Habit habit);
    Task DeleteAsync(int id);
}