using Domain.Entities;

namespace Domain.Interfaces;


public interface IHabitCompletionRepository
{
    Task<HabitCompletion> GetByIdAsync(int id);
    Task<IEnumerable<HabitCompletion>> GetByHabitIdAsync(int habitId);
    Task CreateAsync(HabitCompletion habitCompletion);
    Task UpdateAsync(HabitCompletion habitCompletion);
    Task DeleteAsync(int id);

    Task<IEnumerable<HabitCompletion>> GetUserHistoryHabitCompletionAsync(int habitId, DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null);
}