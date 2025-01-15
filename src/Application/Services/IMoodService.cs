using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Application.Services;

public interface IMoodService
{
    Task<MoodDto> GetByIdAsync(int id, int userId);
    Task<IEnumerable<MoodDto>> GetUserMoodHistoryAsync(int userId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
    Task<MoodDto> CreateMoodAsync(int userId, CreateMoodDto createMoodDto);
    Task<MoodDto> UpdateMoodAsync(int id, int userId, UpdateMoodDto updateMoodDto);
    Task DeleteMoodAsync(int id, int userId);
}