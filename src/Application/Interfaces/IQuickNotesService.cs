using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Application.Interfaces;

public interface IQuickNotesService
{
    Task<QuickNoteDto> GetNoteByIdAsync(int id, int userId);
    Task<IEnumerable<QuickNoteDto>> GetUserNotesAsync(int userId);
    Task<QuickNoteDto> CreateNoteAsync(int userId, CreateQuickNoteDto tag);
    Task<QuickNoteDto> UpdateNoteAsync(int id, int userId, UpdateQuickNoteDto tag);
    Task DeleteNoteAsync(int id, int userId);
}
