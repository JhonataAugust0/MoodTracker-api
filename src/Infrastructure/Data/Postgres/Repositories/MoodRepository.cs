using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Infrastructure.Data.Postgres.Config;
using Microsoft.EntityFrameworkCore;

namespace MoodTracker_back.Infrastructure.Data.Postgres.Repositories
{
    public class MoodRepository : IMoodRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _logger;

        public MoodRepository(ApplicationDbContext context, ILoggingService logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Mood?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.MoodEntries
                    .Include(m => m.Tags)
                    .FirstOrDefaultAsync(m => m.Id == id);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar Mood pelo ID {MoodId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Mood>> GetUserMoodsAsync(int userId)
        {
            try
            {
                return await _context.MoodEntries
                    .Where(m => m.UserId == userId)
                    .OrderByDescending(m => m.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar os moods do usuário {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<Mood>> GetUserHistoryMoodAsync(int moodId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            try
            {
                return await _context.MoodEntries
                    .Include(m => m.Tags)
                    .Where(m => m.Id == moodId &&
                                (!startDate.HasValue || m.Timestamp >= startDate.Value) &&
                                (!endDate.HasValue || m.Timestamp <= endDate.Value.AddDays(1)))
                    .OrderByDescending(m => m.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar o histórico de mood para o MoodId {MoodId}", moodId);
                throw;
            }
        }

        public async Task<IEnumerable<Mood>> GetByUserIdAsync(int userId)
        {
            try
            {
                return await _context.MoodEntries
                    .Include(m => m.Tags)
                    .Where(m => m.UserId == userId)
                    .OrderByDescending(m => m.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar moods por UserId {UserId}", userId);
                throw;
            }
        }

        public async Task CreateAsync(Mood moodBase)
        {
            try
            {
                _context.MoodEntries.Add(moodBase);
                await _context.SaveChangesAsync();
                await _logger.LogInformationAsync("Mood criado com sucesso para o usuário {UserId}", moodBase.UserId);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao criar Mood para o usuário {UserId}", moodBase.UserId);
                throw;
            }
        }

        public async Task UpdateAsync(Mood moodBase)
        {
            try
            {
                _context.Entry(moodBase).State = EntityState.Modified;
                int affected = await _context.SaveChangesAsync();

                if (affected == 0)
                {
                    await _logger.LogWarningAsync("Nenhuma alteração detectada na atualização do Mood {MoodId}", moodBase.Id);
                    throw new DbUpdateConcurrencyException("Mood não encontrado ou nenhuma alteração realizada");
                }

                await _logger.LogInformationAsync("Mood {MoodId} atualizado com sucesso", moodBase.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _logger.LogErrorAsync(ex, "Conflito de concorrência ao atualizar Mood {MoodId}", moodBase.Id);
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao atualizar Mood {MoodId}", moodBase.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var mood = await _context.MoodEntries.FindAsync(id);
                if (mood != null)
                {
                    _context.MoodEntries.Remove(mood);
                    int affected = await _context.SaveChangesAsync();

                    if (affected == 0)
                    {
                        await _logger.LogWarningAsync("Nenhum Mood deletado com o ID {MoodId}", id);
                        throw new DbUpdateConcurrencyException("Mood não encontrado");
                    }

                    await _logger.LogInformationAsync("Mood {MoodId} deletado com sucesso", id);
                }
                else
                {
                    await _logger.LogWarningAsync("Mood com o ID {MoodId} não encontrado para exclusão", id);
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _logger.LogErrorAsync(ex, "Conflito de concorrência ao deletar Mood {MoodId}", id);
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao deletar Mood {MoodId}", id);
                throw;
            }
        }
    }
}
