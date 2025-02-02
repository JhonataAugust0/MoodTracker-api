using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Infrastructure.Data.Postgres.Config;
using Microsoft.EntityFrameworkCore;
using MoodTracker_back.Application.Interfaces;

namespace MoodTracker_back.Infrastructure.Data.Postgres.Repositories
{
    public class HabitCompletionCompletionRepository : IHabitCompletionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _logger;

        public HabitCompletionCompletionRepository(ApplicationDbContext context, ILoggingService logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task<IEnumerable<HabitCompletion>> GetUserHistoryHabitCompletionAsync(int habitId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            try
            {
                return await _context.HabitCompletions
                    .Where(h => h.HabitId == habitId &&
                                (!startDate.HasValue || h.CreatedAt >= startDate.Value) &&
                                (!endDate.HasValue || h.CompletedAt <= endDate.Value.AddDays(1)))
                    .OrderByDescending(h => h.CompletedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar histórico de completions do hábito {HabitId}", habitId);
                throw;
            }
        }

        public async Task<HabitCompletion?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.HabitCompletions
                    .FirstOrDefaultAsync(h => h.Id == id);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar HabitCompletion pelo ID {HabitCompletionId}", id);
                throw;
            }
        }
        
        public async Task<IEnumerable<HabitCompletion>> GetByHabitIdAsync(int habitId)
        {
            try
            {
                return await _context.HabitCompletions
                    .Where(h => h.HabitId == habitId)
                    .OrderByDescending(h => h.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar HabitCompletions para o hábito {HabitId}", habitId);
                throw;
            }
        }

        public async Task CreateAsync(HabitCompletion habitCompletion)
        {
            try
            {
                _context.HabitCompletions.Add(habitCompletion);
                await _context.SaveChangesAsync();
                await _logger.LogInformationAsync("HabitCompletion criado com sucesso com ID {HabitCompletionId} para o Hábito {HabitId}", habitCompletion.Id, habitCompletion.HabitId);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao criar HabitCompletion para o Hábito {HabitId}", habitCompletion.HabitId);
                throw;
            }
        }

        public async Task UpdateAsync(HabitCompletion habitCompletion)
        {
            try
            {
                _context.Entry(habitCompletion).State = EntityState.Modified;
                int affected = await _context.SaveChangesAsync();
                if (affected == 0)
                {
                    await _logger.LogWarningAsync("Nenhuma alteração detectada na atualização do HabitCompletion {HabitCompletionId}", habitCompletion.Id);
                    throw new DbUpdateConcurrencyException("HabitCompletion não encontrado ou nenhuma alteração realizada");
                }
                await _logger.LogInformationAsync("HabitCompletion {HabitCompletionId} atualizado com sucesso", habitCompletion.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _logger.LogErrorAsync(ex, "Conflito de concorrência ao atualizar HabitCompletion {HabitCompletionId}", habitCompletion.Id);
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao atualizar HabitCompletion {HabitCompletionId}", habitCompletion.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var habitCompletion = await _context.HabitCompletions.FindAsync(id);
                if (habitCompletion != null)
                {
                    _context.HabitCompletions.Remove(habitCompletion);
                    int affected = await _context.SaveChangesAsync();
                    if (affected == 0)
                    {
                        await _logger.LogWarningAsync("Nenhum HabitCompletion deletado com o ID {HabitCompletionId}", id);
                        throw new DbUpdateConcurrencyException("HabitCompletion não encontrado");
                    }
                    await _logger.LogInformationAsync("HabitCompletion {HabitCompletionId} deletado com sucesso", id);
                }
                else
                {
                    await _logger.LogWarningAsync("HabitCompletion com o ID {HabitCompletionId} não encontrado para exclusão", id);
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _logger.LogErrorAsync(ex, "Conflito de concorrência ao deletar HabitCompletion {HabitCompletionId}", id);
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao deletar HabitCompletion {HabitCompletionId}", id);
                throw;
            }
        }
    }
}
