using System.Globalization;
using Domain.Entities;
using Domain.Interfaces;
using MoodTracker_back.Infrastructure.Exceptions;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Infrastructure.Adapters;

public class HabitService : IHabitService
{
    private readonly IHabitRepository _habitRepository;
    private readonly IHabitCompletionRepository _habitCompletionRepository;
    private readonly ITagRepository _tagRepository;

    public HabitService(IHabitRepository habitRepository, ITagRepository tagRepository, IHabitCompletionRepository habitCompletionRepository)
    {
        _habitRepository = habitRepository;
        _habitCompletionRepository = habitCompletionRepository;
        _tagRepository = tagRepository;
    }
    
    public async Task<HabitDto> GetByIdAsync(int id, int userId)
    {
        var habit = await _habitRepository.GetByIdAsync(id);
        if (habit == null || habit.UserId != userId)
            throw new NotFoundException("Habit not found");

        return MapToDto(habit);
    }

    public async Task<IEnumerable<HabitDto>> GetUserHabitsAsync(int userId)
    {
        var habits = await _habitRepository.GetUserHabitsAsync(userId);
        return habits.Select(MapToDto);
    }
    
    public async Task<IEnumerable<HabitCompletion>> GetUserHistoryHabitCompletionAsync(int habitId,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null)
    {
        if (startDate > endDate)
            throw new ArgumentException("A data inicial não pode ser posterior à data final.");

        var history = await _habitCompletionRepository.GetUserHistoryHabitCompletionAsync(habitId, startDate, endDate);
        return history;
    }

    public async Task<HabitDto> CreateHabitAsync(int userId, CreateHabitDto createHabitDto)
    {
        var habit = new Habit
        {
            UserId = userId,
            Name = createHabitDto.Name,
            Description = createHabitDto.Description,
            CreatedAt = createHabitDto.CreatedAt ?? DateTimeOffset.UtcNow, 
            UpdatedAt = createHabitDto.CreatedAt ?? DateTimeOffset.UtcNow, 
            IsActive = createHabitDto.IsActive ?? true,
            FrequencyType = createHabitDto.FrequencyType,
            FrequencyTarget = createHabitDto.FrequencyTarget,
            Color = createHabitDto.Color
        };

        if (createHabitDto.TagIds.Any())
        {
            var tags = await _tagRepository.GetByIdsAsync(createHabitDto.TagIds);
            foreach (var tag in tags.Where(t => t.UserId == userId))
            {
                habit.Tags.Add(tag);
            }
        }

        await _habitRepository.CreateAsync(habit);
        return MapToDto(habit);
    }
    
    public async Task<HabitCompletionDto> LogHabitAsync(int userId, LogHabitCompletionDto habitCompletionDto)
    {
        var habit = await _habitRepository.GetByIdAsync(habitCompletionDto.HabitId);
        if (habit == null || habit.UserId != userId)
        {
            throw new NotFoundException("Habit not found or user is not authorized.");
        }

        var habitCompletion = new HabitCompletion
        {
            HabitId = habit.Id,
            CompletedAt = habitCompletionDto.CompletedAt,
            Notes = habitCompletionDto.Notes,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _habitCompletionRepository.CreateAsync(habitCompletion);
    
        var result = new HabitCompletionDto
        {
            Id = habitCompletion.Id,
            HabitId = habitCompletion.HabitId,
            CompletedAt = habitCompletion.CompletedAt,
            Notes = habitCompletion.Notes,
        };
        return result;
    }

    public async Task<HabitDto> UpdateHabitAsync(int id, int userId, UpdateHabitDto updateHabitDto)
    {
        var habit = await _habitRepository.GetByIdAsync(id);
        if (habit == null || habit.UserId != userId)
            throw new NotFoundException("Habit not found");

        if (updateHabitDto.Name != null)
            habit.Name = updateHabitDto.Name;
        
        if (updateHabitDto.Description != null)
            habit.Description = updateHabitDto.Description;
        
        if (updateHabitDto.IsActive != null)
            habit.IsActive = updateHabitDto.IsActive.Value;

        if (updateHabitDto.TagIds != null)
        {
            habit.Tags.Clear();
            var tags = await _tagRepository.GetByIdsAsync(updateHabitDto.TagIds);
            foreach (var tag in tags.Where(t => t.UserId == userId))
            {
                habit.Tags.Add(tag);
            }
        }

        habit.UpdatedAt = DateTimeOffset.UtcNow;
        await _habitRepository.UpdateAsync(habit);
        return MapToDto(habit);
    }

    public async Task DeleteHabitAsync(int id, int userId)
    {
        var mood = await _habitRepository.GetByIdAsync(id);
        if (mood == null || mood.UserId != userId)
            throw new NotFoundException("Habit not found");

        await _habitRepository.DeleteAsync(id);
    }

    private static HabitDto MapToDto(Habit habit)
    {
        return new HabitDto
        {
            Id = habit.Id,
            UserId = habit.UserId,
            Name = habit.Name,
            Description = habit.Description,
            CreatedAt = habit.CreatedAt,
            UpdatedAt = habit.UpdatedAt,
            IsActive = habit.IsActive,
            FrequencyType = habit.FrequencyType,
            FrequencyTarget = habit.FrequencyTarget,
            Color = habit.Color,
            Tags = habit.Tags.Select(tag => new TagDto
            {
                Id = tag.Id,
                Name = tag.Name
            }).ToList()
        };
    }
}