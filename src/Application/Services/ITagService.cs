using Domain.Entities;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Application.Services;

public interface ITagService
{
    Task<TagDto> GetTagByIdAsync(int id, int userId);
    Task<IEnumerable<Tag>> GetUserTagsAsync(int userId);
    Task<TagDto> CreateTagAsync(int userId, CreateTagDto tag);
    Task<TagDto> UpdateTagAsync(int id, int userId, UpdateTagDto tag);
    Task DeleteTagAsync(int id, int userId);
}
