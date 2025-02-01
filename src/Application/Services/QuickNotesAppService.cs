using Domain.Entities;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Domain.Exceptions;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Application.Services;


public class QuickNotesAppService : IQuickNotesService
{
    private readonly IQuickNoteRepository _notesRepository;
    private readonly ITagRepository _tagRepository;

    public QuickNotesAppService(IQuickNoteRepository notesRepository, ITagRepository tagRepository)
    {
        _notesRepository = notesRepository;
        _tagRepository = tagRepository;
    }
    
    public async Task<QuickNoteDto> GetNoteByIdAsync(int id, int userId)
    {
        var note = await _notesRepository.GetByIdAsync(id);
        if (note == null || note.UserId != userId)
        {
            throw new NotFoundException("Note not found");
        }

        return MapToDto(note);
    }

    public async Task<IEnumerable<QuickNoteDto>> GetUserNotesAsync(int userId)
    {
        var notes = await _notesRepository.GetByUserIdAsync(userId);
        return notes.Select(MapToDto);
    }
    
    public async Task<QuickNoteDto> CreateNoteAsync(int userId, CreateQuickNoteDto createQuickNoteDto)
    {
        var note = new QuickNote()
        {
            UserId = userId,
            Content = createQuickNoteDto.Content,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        if (createQuickNoteDto.TagIds.Any())
        {
            var tags = await _tagRepository.GetByIdsAsync(createQuickNoteDto.TagIds);
            foreach (var tag in tags.Where(t => t.UserId == userId))
            {
                note.Tags.Add(tag);
            }
        }
        
        await _notesRepository.CreateAsync(note);
        return MapToDto(note);
    }

    public async Task<QuickNoteDto> UpdateNoteAsync(int id, int userId, UpdateQuickNoteDto updateQuickNoteDto)
    {
        var note = await _notesRepository.GetByIdAsync(id);
        if (note == null || note.UserId != userId)
        {
             throw new NotFoundException("Mood not found");
        }

        if (updateQuickNoteDto.Content != null)
        {
            note.Content = updateQuickNoteDto.Content;
        }

        if (updateQuickNoteDto.TagIds != null)
        {
            note.Tags.Clear();
            var tags = await _tagRepository.GetByIdsAsync(updateQuickNoteDto.TagIds);
            foreach (var tag in tags.Where(t => t.UserId == userId))
            {
                note.Tags.Add(tag);
            };
        }

        await _notesRepository.UpdateAsync(note);
        return MapToDto(note);
    }

    public async Task DeleteNoteAsync(int id, int userId)
    {
        var note = await _notesRepository.GetByIdAsync(id);
        if (note == null || note.UserId != userId)
        {   
            throw new NotFoundException("Note not found");
        }
        await _notesRepository.DeleteAsync(id);
    }
    
    private static QuickNoteDto MapToDto(QuickNote tag)
    {
        return new QuickNoteDto
        {
            Id = tag.Id,
            UserId = tag.UserId,
            Content = tag.Content,
            CreatedAt = tag.CreatedAt,
            UpdatedAt = tag.UpdatedAt,
            IsDeleted = tag.IsDeleted,
            Tags = new List<TagDto>()
        };
    }
}