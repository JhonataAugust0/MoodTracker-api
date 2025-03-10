using MoodTracker_back.Domain.Entities;

namespace MoodTracker_back.Domain.Interfaces;

public interface ITagRepository
{
    Task<Tag> GetByIdAsync(int id);
    Task CreateAsync(Tag tag);
    Task UpdateAsync(Tag tag);
    Task DeleteAsync(int id);

    Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<int> ids);
    Task<IEnumerable<Tag>> GetUserTagsAsync(int userId);
}