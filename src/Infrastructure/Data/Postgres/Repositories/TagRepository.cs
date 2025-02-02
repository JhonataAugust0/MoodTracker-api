using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Infrastructure.Data.Postgres.Config;
using Microsoft.EntityFrameworkCore;
using MoodTracker_back.Application.Interfaces;

namespace MoodTracker_back.Infrastructure.Data.Postgres.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _logger;

        public TagRepository(ApplicationDbContext context, ILoggingService logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Tag?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar Tag pelo ID {TagId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Tag>> GetUserTagsAsync(int userId)
        {
            try
            {
                return await _context.Tags
                    .Where(t => t.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar Tags (sem Include) pelo UserId {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<int> ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                    return Enumerable.Empty<Tag>();

                return await _context.Tags
                    .Where(t => ids.Contains(t.Id))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar Tags pelos IDs");
                throw;
            }
        }

        public async Task CreateAsync(Tag tag)
        {
            try
            {
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
                await _logger.LogInformationAsync("Tag criada com ID {TagId} para o usuário {UserId}", tag.Id, tag.UserId);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao criar Tag para o usuário {UserId}", tag.UserId);
                throw;
            }
        }

        public async Task UpdateAsync(Tag tag)
        {
            try
            {
                _context.Entry(tag).State = EntityState.Modified;
                var affected = await _context.SaveChangesAsync();

                if (affected == 0)
                {
                    await _logger.LogWarningAsync("Nenhuma alteração detectada na atualização da Tag {TagId}", tag.Id);
                    throw new DbUpdateConcurrencyException("Tag não encontrada ou nenhuma alteração realizada");
                }

                await _logger.LogInformationAsync("Tag {TagId} atualizada", tag.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _logger.LogErrorAsync(ex, "Conflito de concorrência ao atualizar Tag {TagId}", tag.Id);
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao atualizar Tag {TagId}", tag.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var tag = await _context.Tags.FindAsync(id);
                if (tag != null)
                {
                    _context.Tags.Remove(tag);
                    var affected = await _context.SaveChangesAsync();

                    if (affected == 0)
                    {
                        await _logger.LogWarningAsync("Nenhuma Tag deletada com ID {TagId}", id);
                        throw new DbUpdateConcurrencyException("Tag não encontrada");
                    }

                    await _logger.LogInformationAsync("Tag {TagId} deletada", id);
                }
                else
                {
                    await _logger.LogWarningAsync("Tag com ID {TagId} não encontrada para exclusão", id);
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _logger.LogErrorAsync(ex, "Conflito de concorrência ao deletar Tag {TagId}", id);
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao deletar Tag {TagId}", id);
                throw;
            }
        }
    }
}
