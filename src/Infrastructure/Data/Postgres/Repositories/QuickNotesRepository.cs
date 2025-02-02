using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Infrastructure.Data.Postgres.Config;
using Microsoft.EntityFrameworkCore;
using MoodTracker_back.Application.Interfaces;

namespace MoodTracker_back.Infrastructure.Data.Postgres.Repositories
{
    public class QuickNotesRepository : IQuickNoteRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _logger;

        public QuickNotesRepository(ApplicationDbContext context, ILoggingService logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<QuickNote?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.QuickNotes.FirstOrDefaultAsync(q => q.Id == id);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar QuickNote pelo ID {QuickNoteId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<QuickNote>> GetByUserIdAsync(int userId)
        {
            try
            {
                return await _context.QuickNotes
                    .Where(q => q.UserId == userId)
                    .OrderByDescending(q => q.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar QuickNotes pelo UserId {UserId}", userId);
                throw;
            }
        }

        public async Task CreateAsync(QuickNote noteBase)
        {
            try
            {
                _context.QuickNotes.Add(noteBase);
                await _context.SaveChangesAsync();
                await _logger.LogInformationAsync("QuickNote criada com ID {QuickNoteId} para o usuário {UserId}", noteBase.Id, noteBase.UserId);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao criar QuickNote para o usuário {UserId}", noteBase.UserId);
                throw;
            }
        }

        public async Task UpdateAsync(QuickNote noteBase)
        {
            try
            {
                _context.Entry(noteBase).State = EntityState.Modified;
                int affected = await _context.SaveChangesAsync();

                if (affected == 0)
                {
                    await _logger.LogWarningAsync("Nenhuma alteração detectada na atualização da QuickNote {QuickNoteId}", noteBase.Id);
                    throw new DbUpdateConcurrencyException("QuickNote não encontrada ou nenhuma alteração realizada");
                }

                await _logger.LogInformationAsync("QuickNote {QuickNoteId} atualizada", noteBase.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _logger.LogErrorAsync(ex, "Conflito de concorrência ao atualizar QuickNote {QuickNoteId}", noteBase.Id);
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao atualizar QuickNote {QuickNoteId}", noteBase.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var note = await _context.QuickNotes.FindAsync(id);
                if (note != null)
                {
                    _context.QuickNotes.Remove(note);
                    int affected = await _context.SaveChangesAsync();

                    if (affected == 0)
                    {
                        await _logger.LogWarningAsync("Nenhuma QuickNote deletada com ID {QuickNoteId}", id);
                        throw new DbUpdateConcurrencyException("QuickNote não encontrada");
                    }

                    await _logger.LogInformationAsync("QuickNote {QuickNoteId} deletada", id);
                }
                else
                {
                    await _logger.LogWarningAsync("QuickNote com ID {QuickNoteId} não encontrada para exclusão", id);
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _logger.LogErrorAsync(ex, "Conflito de concorrência ao deletar QuickNote {QuickNoteId}", id);
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao deletar QuickNote {QuickNoteId}", id);
                throw;
            }
        }
    }
}
