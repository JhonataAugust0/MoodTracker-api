using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Application.Interfaces;

public interface IHabitService
{
    Task<HabitDto> GetByIdAsync(int id, int userId);
    Task<IEnumerable<HabitDto>> GetUserHabitsAsync(int userId);
    Task<HabitDto> CreateHabitAsync(int userId, CreateHabitDto createHabitDto);
    Task<HabitCompletionDto> LogHabitAsync(int userId, LogHabitCompletionDto habitCompletion);
    Task<HabitDto> UpdateHabitAsync(int id, int userId, UpdateHabitDto updateHabitDto);
    Task DeleteHabitAsync(int id, int userId);
    Task<IEnumerable<HabitCompletion>> GetUserHistoryHabitCompletionAsync(int habitId, DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null);
}