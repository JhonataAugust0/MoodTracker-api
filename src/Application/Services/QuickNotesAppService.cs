using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Domain.Exceptions;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Application.Services
{
    public class QuickNotesAppService : IQuickNotesService
    {
        private readonly IQuickNoteRepository _notesRepository;
        private readonly ITagRepository _tagRepository;
        private readonly ICryptographService _cryptographService;
        private readonly ILoggingService _logger;

        public QuickNotesAppService(
            IQuickNoteRepository notesRepository, 
            ITagRepository tagRepository,
            ICryptographService cryptographService,
            ILoggingService logger)
        {
            _notesRepository = notesRepository ?? throw new ArgumentNullException(nameof(notesRepository));
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
            _cryptographService = cryptographService ?? throw new ArgumentNullException(nameof(cryptographService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task<QuickNoteDto> GetNoteByIdAsync(int id, int userId)
        {
            try
            {
                var note = await _notesRepository.GetByIdAsync(id);
                if (note == null || note.UserId != userId)
                {
                    throw new NotFoundException("Note not found");
                }
                return MapToDto(note);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar a nota com ID {NoteId} para o usuário {UserId}", id, userId);
                throw new ApplicationException("Erro ao buscar a nota. Tente novamente mais tarde.", ex);
            }
        }

        public async Task<IEnumerable<QuickNoteDto>> GetUserNotesAsync(int userId)
        {
            try
            {
                var notes = await _notesRepository.GetByUserIdAsync(userId);
                return notes.Select(MapToDto);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar as notas para o usuário {UserId}", userId);
                throw new ApplicationException("Erro ao buscar as notas. Tente novamente mais tarde.", ex);
            }
        }
        
        public async Task<QuickNoteDto> CreateNoteAsync(int userId, CreateQuickNoteDto createQuickNoteDto)
        {
            if (createQuickNoteDto == null)
                throw new ArgumentNullException(nameof(createQuickNoteDto));
            if (string.IsNullOrWhiteSpace(createQuickNoteDto.Content))
                throw new ArgumentException("O conteúdo da nota é obrigatório.", nameof(createQuickNoteDto.Content));

            try
            {
                var note = new QuickNote()
                {
                    UserId = userId,
                    Content = _cryptographService.Encrypt(createQuickNoteDto.Content),
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
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao criar a nota para o usuário {UserId}", userId);
                throw new ApplicationException("Erro ao criar a nota. Tente novamente mais tarde.", ex);
            }
        }

        public async Task<QuickNoteDto> UpdateNoteAsync(int id, int userId, UpdateQuickNoteDto updateQuickNoteDto)
        {
            if (updateQuickNoteDto == null)
                throw new ArgumentNullException(nameof(updateQuickNoteDto));

            try
            {
                var note = await _notesRepository.GetByIdAsync(id);
                if (note == null || note.UserId != userId)
                {
                    throw new NotFoundException("Note not found");
                }

                if (!string.IsNullOrWhiteSpace(updateQuickNoteDto.Content))
                {
                    note.Content = _cryptographService.Encrypt(updateQuickNoteDto.Content);
                    note.UpdatedAt = DateTimeOffset.UtcNow;
                }

                if (updateQuickNoteDto.TagIds != null)
                {
                    note.Tags.Clear();
                    var tags = await _tagRepository.GetByIdsAsync(updateQuickNoteDto.TagIds);
                    foreach (var tag in tags.Where(t => t.UserId == userId))
                    {
                        note.Tags.Add(tag);
                    }
                }

                await _notesRepository.UpdateAsync(note);
                return MapToDto(note);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao atualizar a nota com ID {NoteId} para o usuário {UserId}", id, userId);
                throw new ApplicationException("Erro ao atualizar a nota. Tente novamente mais tarde.", ex);
            }
        }

        public async Task DeleteNoteAsync(int id, int userId)
        {
            try
            {
                var note = await _notesRepository.GetByIdAsync(id);
                if (note == null || note.UserId != userId)
                {   
                    throw new NotFoundException("Note not found");
                }
                await _notesRepository.DeleteAsync(id);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao deletar a nota com ID {NoteId} para o usuário {UserId}", id, userId);
                throw new ApplicationException("Erro ao deletar a nota. Tente novamente mais tarde.", ex);
            }
        }
        
        private QuickNoteDto MapToDto(QuickNote noteBase)
        {
            return new QuickNoteDto
            {
                Id = noteBase.Id,
                UserId = noteBase.UserId,
                Content = _cryptographService.Decrypt(noteBase.Content),
                CreatedAt = noteBase.CreatedAt,
                UpdatedAt = noteBase.UpdatedAt,
                IsDeleted = noteBase.IsDeleted,
                Tags = new List<TagDto>()
            };
        }
    }
}
