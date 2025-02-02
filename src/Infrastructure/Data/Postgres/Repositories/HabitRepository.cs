using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Infrastructure.Data.Postgres.Config;
using Microsoft.EntityFrameworkCore;

namespace MoodTracker_back.Infrastructure.Data.Postgres.Repositories
{
    public class HabitRepository : IHabitRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _logger;

        public HabitRepository(ApplicationDbContext context, ILoggingService logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Habit?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Habits
                    .Include(h => h.Tags)
                    .FirstOrDefaultAsync(h => h.Id == id);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar Habit pelo ID {HabitId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Habit>> GetUserHabitsAsync(int userId)
        {
            try
            {
                return await _context.Habits
                    .Where(h => h.UserId == userId)
                    .OrderByDescending(h => h.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar habits do usuário {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<Habit>> GetByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Habits
                    .Include(h => h.Tags)
                    .Where(h => h.UserId == userId)
                    .OrderByDescending(h => h.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar habits por UserId {UserId}", userId);
                throw;
            }
        }

        public async Task CreateAsync(Habit habitBase)
        {
            try
            {
                _context.Habits.Add(habitBase);
                await _context.SaveChangesAsync();
                await _logger.LogInformationAsync("Habit criado com sucesso com ID {HabitId} para o usuário {UserId}", habitBase.Id, habitBase.UserId);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao criar Habit para o usuário {UserId}", habitBase.UserId);
                throw;
            }
        }

        public async Task UpdateAsync(Habit habitBase)
        {
            try
            {
                _context.Entry(habitBase).State = EntityState.Modified;
                int affected = await _context.SaveChangesAsync();

                if (affected == 0)
                {
                    await _logger.LogWarningAsync("Nenhuma alteração detectada na atualização do Habit {HabitId}", habitBase.Id);
                    throw new DbUpdateConcurrencyException("Habit não encontrado ou nenhuma alteração realizada");
                }

                await _logger.LogInformationAsync("Habit {HabitId} atualizado com sucesso", habitBase.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _logger.LogErrorAsync(ex, "Conflito de concorrência ao atualizar Habit {HabitId}", habitBase.Id);
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao atualizar Habit {HabitId}", habitBase.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var habit = await _context.Habits.FindAsync(id);
                if (habit != null)
                {
                    _context.Habits.Remove(habit);
                    int affected = await _context.SaveChangesAsync();

                    if (affected == 0)
                    {
                        await _logger.LogWarningAsync("Nenhum Habit deletado com o ID {HabitId}", id);
                        throw new DbUpdateConcurrencyException("Habit não encontrado");
                    }

                    await _logger.LogInformationAsync("Habit {HabitId} deletado com sucesso", id);
                }
                else
                {
                    await _logger.LogWarningAsync("Habit com o ID {HabitId} não encontrado para exclusão", id);
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _logger.LogErrorAsync(ex, "Conflito de concorrência ao deletar Habit {HabitId}", id);
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao deletar Habit {HabitId}", id);
                throw;
            }
        }
    }
}
