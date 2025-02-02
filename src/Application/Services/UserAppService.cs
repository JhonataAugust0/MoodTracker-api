using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Domain.Exceptions;
using MoodTracker_back.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MoodTracker_back.Application.Services
{
    public class UserAppService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly ILoggingService _logger;

        public UserAppService(
            IUserRepository userRepository,
            IPasswordService passwordService,
            ILoggingService logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<User> RegisterUserAsync(string email, string password, string? name)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email é obrigatório.", nameof(email));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Senha é obrigatória.", nameof(password));

            try
            {
                if (await _userRepository.EmailExistsAsync(email))
                {
                    throw new ValidationException("Email já existe");
                }

                var passwordHash = _passwordService.HashPassword(password, out string salt);

                var user = new User
                {
                    Email = email,
                    Name = name,
                    PasswordHash = $"{passwordHash}:{salt}",
                };

                await _userRepository.CreateAsync(user);
                return user;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao registrar o usuário com email {Email}", email);
                throw new ApplicationException("Ocorreu um erro ao registrar o usuário. Tente novamente mais tarde.", ex);
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    throw new NotFoundException("Usuário não encontrado");
                }

                return user;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar usuário pelo ID {UserId}", id);
                throw new ApplicationException("Erro ao buscar usuário. Tente novamente mais tarde.", ex);
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email é obrigatório.", nameof(email));

            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                return user;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar usuário pelo email {Email}", email);
                throw new ApplicationException("Erro ao buscar usuário. Tente novamente mais tarde.", ex);
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email é obrigatório.", nameof(email));

            try
            {
                return await _userRepository.EmailExistsAsync(email);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao verificar existência do email {Email}", email);
                throw new ApplicationException("Erro ao verificar existência do email. Tente novamente mais tarde.", ex);
            }
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("Refresh token é obrigatório.", nameof(refreshToken));

            try
            {
                return await _userRepository.GetUserByRefreshTokenAsync(refreshToken);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar usuário pelo refresh token");
                throw new ApplicationException("Erro ao buscar usuário. Tente novamente mais tarde.", ex);
            }
        }

        public async Task<IEnumerable<User>> GetInactiveUsers(CancellationToken stoppingToken)
        {
            try
            {
                return await _userRepository.GetInactiveUsers(stoppingToken);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao buscar usuários inativos");
                throw new ApplicationException("Erro ao buscar usuários inativos. Tente novamente mais tarde.", ex);
            }
        }

        public async Task UpdateUserLastNotifiedAsync(int userId, DateTime lastNotified, CancellationToken stoppingToken)
        {
            try
            {
                await _userRepository.UpdateLastNotifiedAsync(userId, lastNotified, stoppingToken);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao atualizar a última notificação do usuário {UserId}", userId);
                throw new ApplicationException("Erro ao atualizar a notificação do usuário. Tente novamente mais tarde.", ex);
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                await _userRepository.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao atualizar o usuário {UserId}", user.Id);
                throw new ApplicationException("Erro ao atualizar usuário. Tente novamente mais tarde.", ex);
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    throw new NotFoundException("Usuário não encontrado");
                }

                await _userRepository.DeleteAsync(user);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao deletar o usuário {UserId}", id);
                throw new ApplicationException("Erro ao deletar usuário. Tente novamente mais tarde.", ex);
            }
        }
    }
}
