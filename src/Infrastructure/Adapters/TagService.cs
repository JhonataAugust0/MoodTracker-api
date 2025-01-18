using Domain.Entities;
using Domain.Interfaces;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Infrastructure.Exceptions;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Infrastructure.Adapters;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }
    
    public async Task<TagDto> GetTagByIdAsync(int id, int userId)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null || tag.UserId != userId)
        {
            throw new NotFoundException("Tag not found");
        }

        return MapToDto(tag);
    }

    public async Task<IEnumerable<Tag>> GetUserTagsAsync(int userId)
    {
        return await _tagRepository.GetUserTagsAsync(userId);
    }
    
    public async Task<TagDto> CreateTagAsync(int userId, CreateTagDto createTagDto)
    {
        var tag = new Tag()
        {
            UserId = userId,
            Name = createTagDto.Name,
            Color = createTagDto.Color,
            CreatedAt = createTagDto.Timestamp ?? DateTimeOffset.UtcNow
        };

        await _tagRepository.CreateAsync(tag);
        return MapToDto(tag);
    }

    public async Task<TagDto> UpdateTagAsync(int id, int userId, UpdateTagDto updateTagDto)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null || tag.UserId != userId)
        {
             throw new NotFoundException("Mood not found");
        }

        if (updateTagDto.Name != null)
        {
            tag.Name = updateTagDto.Name;
        }

        if (updateTagDto.Color != null)
        {
            tag.Color = updateTagDto.Color;
        }

        await _tagRepository.UpdateAsync(tag);
        return MapToDto(tag);
    }

    public async Task DeleteTagAsync(int id, int userId)
    {
        var tag = await _tagRepository.GetByIdAsync(id);
        if (tag == null || tag.UserId != userId)
        {   
            throw new NotFoundException("Tag not found");
        }
        await _tagRepository.DeleteAsync(id);
    }
    
    private static TagDto MapToDto(Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            UserId = tag.UserId,
            Name = tag.Name,
            Color = tag.Color,
            CreatedAt = tag.CreatedAt,
        };
    }
}