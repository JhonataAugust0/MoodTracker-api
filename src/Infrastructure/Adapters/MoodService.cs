using System.Globalization;
using Domain.Entities;
using Domain.Interfaces;
using MoodTracker_back.Infrastructure.Exceptions;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Infrastructure.Adapters;

public class MoodService : IMoodService
{
    private readonly IMoodRepository _moodRepository;
    private readonly IUserService _userService;
    private readonly ITagRepository _tagRepository;

    public MoodService(IMoodRepository moodRepository, ITagRepository tagRepository, IUserService userService)
    {
        _moodRepository = moodRepository;
        _tagRepository = tagRepository;
        _userService = userService;
    }
    
    public async Task<MoodDto> GetByIdAsync(int id, int userId)
    {
        var mood = await _moodRepository.GetByIdAsync(id);
        if (mood == null || mood.UserId != userId)
            throw new NotFoundException("Mood not found");

        return MapToDto(mood);
    }
    
    public async Task<IEnumerable<MoodDto>> GetUserMoodsAsync(int userId)
    {
        var habits = await _moodRepository.GetUserMoodsAsync(userId);
        return habits.Select(MapToDto);
    }

    public async Task<IEnumerable<MoodDto>> GetUserMoodHistoryAsync(int moodId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        if (startDate > endDate)
            throw new ArgumentException("A data inicial não pode ser posterior à data final.");
        
        var moods = await _moodRepository.GetUserHistoryMoodAsync(moodId, startDate, endDate);
        return moods.Select(MapToDto);
    }

    public async Task<MoodDto> CreateMoodAsync(int userId, CreateMoodDto createMoodDto)
    {
        var mood = new Mood
        {
            UserId = userId,
            MoodType = createMoodDto.MoodType,
            Intensity = createMoodDto.Intensity,
            Notes = createMoodDto.Notes,
            Timestamp = createMoodDto.Timestamp ?? DateTimeOffset.UtcNow,
        };

        var user = await _userService.GetUserByIdAsync(userId);
        user.LastMoodEntry = createMoodDto.Timestamp ?? DateTimeOffset.UtcNow;
        user.LastLogin = createMoodDto.Timestamp ?? DateTimeOffset.UtcNow;
        user.LastNotified = createMoodDto.Timestamp ?? DateTimeOffset.UtcNow;
        await _userService.UpdateUserAsync(user);

        if (createMoodDto.TagIds.Any())
        {
            var tags = await _tagRepository.GetByIdsAsync(createMoodDto.TagIds);
            foreach (var tag in tags.Where(t => t.UserId == userId))
            {
                mood.Tags.Add(tag);
            }
        }

        await _moodRepository.CreateAsync(mood);
        return MapToDto(mood);
    }

    public async Task<MoodDto> UpdateMoodAsync(int id, int userId, UpdateMoodDto updateMoodDto)
    {
        var mood = await _moodRepository.GetByIdAsync(id);
        if (mood == null || mood.UserId != userId)
            throw new NotFoundException("Mood not found");

        if (updateMoodDto.MoodType != null)
            mood.MoodType = updateMoodDto.MoodType;
        
        if (updateMoodDto.Intensity.HasValue)
            mood.Intensity = updateMoodDto.Intensity.Value;
        
        if (updateMoodDto.Notes != null)
            mood.Notes = updateMoodDto.Notes;

        if (updateMoodDto.TagIds != null)
        {
            mood.Tags.Clear();
            var tags = await _tagRepository.GetByIdsAsync(updateMoodDto.TagIds);
            foreach (var tag in tags.Where(t => t.UserId == userId))
            {
                mood.Tags.Add(tag);
            }
        }

        mood.UpdatedAt = DateTimeOffset.UtcNow;
        await _moodRepository.UpdateAsync(mood);
        return MapToDto(mood);
    }

    public async Task DeleteMoodAsync(int id, int userId)
    {
        var mood = await _moodRepository.GetByIdAsync(id);
        if (mood == null || mood.UserId != userId)
            throw new NotFoundException("Mood not found");

        await _moodRepository.DeleteAsync(id);
    }

    private static MoodDto MapToDto(Mood mood)
    {
        return new MoodDto
        {
            Id = mood.Id,
            MoodType = mood.MoodType,
            Intensity = mood.Intensity,
            Notes = mood.Notes,
            Timestamp = mood.Timestamp,
            TagIds = mood.Tags.Select(t => t.Id).ToList()
        };
    }
}