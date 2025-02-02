using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Infrastructure.Data.Postgres.Config;
using Microsoft.EntityFrameworkCore;
using MoodTracker_back.Application.Interfaces;

namespace MoodTracker_back.Infrastructure.Data.Postgres.Repositories
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _logger;

        public PasswordResetTokenRepository(ApplicationDbContext context, ILoggingService logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PasswordResetToken?> GetValidTokenAsync(int userId, string token)
        {
            try
            {
                return await _context.PasswordResetTokens
                    .FirstOrDefaultAsync(t =>
                        t.UserId == userId &&
                        t.Token == token &&
                        !t.Used &&
                        t.ExpiresAt > DateTimeOffset.UtcNow);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar token válido para o usuário {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<PasswordResetToken>> GetExpiredTokensAsync()
        {
            try
            {
                return await _context.PasswordResetTokens
                    .Where(t => !t.Used && t.ExpiresAt < DateTimeOffset.UtcNow)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar tokens expirados");
                throw;
            }
        }

        public async Task AddAsync(PasswordResetToken token)
        {
            try
            {
                await _context.PasswordResetTokens.AddAsync(token);
                await _logger.LogInformationAsync("Token de redefinição de senha adicionado para o usuário {UserId}", token.UserId);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao adicionar token para o usuário {UserId}", token.UserId);
                throw;
            }
        }

        public async Task MarkAsUsedAsync(PasswordResetToken token)
        {
            try
            {
                token.Used = true;
                _context.PasswordResetTokens.Update(token);
                await _logger.LogInformationAsync("Token {Token} marcado como usado para o usuário {UserId}", token.Token, token.UserId);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao marcar token {Token} como usado para o usuário {UserId}", token.Token, token.UserId);
                throw;
            }
        }

        public async Task RemoveRangeAsync(IEnumerable<PasswordResetToken> tokens)
        {
            try
            {
                _context.PasswordResetTokens.RemoveRange(tokens);
                await _logger.LogInformationAsync("Tokens de redefinição de senha removidos: {Count}", tokens.Count());
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao remover tokens de redefinição de senha");
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _logger.LogInformationAsync("Alterações salvas com sucesso no banco de dados");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao salvar alterações no banco de dados");
                throw;
            }
        }
    }
}
