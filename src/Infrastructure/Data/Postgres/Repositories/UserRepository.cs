using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Infrastructure.Data.Postgres.Config;
using Microsoft.EntityFrameworkCore;
using MoodTracker_back.Application.Interfaces;

namespace MoodTracker_back.Infrastructure.Data.Postgres.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILoggingService _logger;

    public UserRepository(ApplicationDbContext context, ILoggingService loggingService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("O email não pode ser nulo ou vazio.", nameof(email));

        try
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, "Erro ao buscar usuário pelo email {Email}", email);
            throw; 
        }
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, "Erro ao buscar usuário pelo ID {UserId}", id);
            throw;
        }
    }

    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("O refresh token não pode ser nulo ou vazio.", nameof(refreshToken));

        try
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken));
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, "Erro ao buscar usuário pelo refresh token");
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetInactiveUsers(CancellationToken stoppingToken)
    {
        try
        {
            var cutoff = DateTime.UtcNow.AddDays(-3);
            return await _context.Users
                .Where(u => u.LastMoodEntry < cutoff &&
                            (!u.LastNotified.HasValue || u.LastNotified < cutoff))
                .ToListAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, "Erro ao buscar usuários inativos");
            throw;
        }
    }

    public async Task UpdateLastNotifiedAsync(int userId, DateTime lastNotified, CancellationToken stoppingToken)
    {
        try
        {
            var user = await _context.Users.FindAsync(new object[] { userId }, stoppingToken);
            if (user != null)
            {
                user.LastNotified = lastNotified;
                await _context.SaveChangesAsync(stoppingToken);
                await _logger.LogInformationAsync("Usuário {UserId} notificado em {LastNotified}", userId, lastNotified);
            }
            else
            {
                await _logger.LogWarningAsync("Usuário {UserId} não encontrado para atualização de notificação", userId);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, "Erro ao atualizar última notificação do usuário {UserId}", userId);
            throw;
        }
    }

    public async Task CreateAsync(User user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await _logger.LogInformationAsync("Usuário {Email} criado com ID {UserId}", user.Email, user.Id);
        }
        catch (DbUpdateException dbEx)
        {
            await _logger.LogErrorAsync(dbEx, "Erro de atualização ao criar usuário {Email}", user.Email);
            throw;
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, "Erro inesperado ao criar usuário {Email}", user.Email);
            throw;
        }
    }

    public async Task UpdateAsync(User user)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));

        try
        {
            _context.Entry(user).State = EntityState.Modified;
            var affected = await _context.SaveChangesAsync();

            if (affected == 0)
            {
                await _logger.LogWarningAsync("Nenhuma alteração detectada na atualização do usuário {UserId}", user.Id);
                throw new DbUpdateConcurrencyException("Usuário não encontrado ou nenhum dado foi alterado");
            }

            await _logger.LogInformationAsync("Usuário {UserId} atualizado", user.Id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _logger.LogErrorAsync(ex, "Conflito de concorrência ao atualizar usuário {UserId}", user.Id);
            throw;
        }
        catch (DbUpdateException ex)
        {
            await _logger.LogErrorAsync(ex, "Erro de atualização ao tentar modificar o usuário {UserId}", user.Id);
            throw;
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, "Erro inesperado ao atualizar usuário {UserId}", user.Id);
            throw;
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("O email não pode ser nulo ou vazio.", nameof(email));

        try
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, "Erro ao verificar existência do email {Email}", email);
            throw;
        }
    }

    public async Task DeleteAsync(User user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        try
        {
            _context.Users.Remove(user);
            int affected = await _context.SaveChangesAsync();

            if (affected == 0)
            {
                await _logger.LogWarningAsync("Nenhum usuário deletado com ID {UserId}", user.Id);
                throw new DbUpdateConcurrencyException("Usuário não encontrado ou já removido");
            }

            await _logger.LogInformationAsync("Usuário {UserId} deletado", user.Id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _logger.LogErrorAsync(ex, "Conflito de concorrência ao deletar usuário {UserId}", user.Id);
            throw;
        }
        catch (DbUpdateException ex)
        {
            await _logger.LogErrorAsync(ex, "Erro de atualização ao tentar deletar usuário {UserId}", user.Id);
            throw;
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, "Erro inesperado ao deletar usuário {UserId}", user.Id);
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            return await _context.Users.ToListAsync();
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(ex, "Erro ao buscar todos os usuários");
            throw;
        }
    }
}
