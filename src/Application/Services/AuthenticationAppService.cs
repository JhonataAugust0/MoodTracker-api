using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Application.Dtos;
using MoodTracker_back.Application.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoodTracker_back.Application.Services
{
    public class AuthenticationAppService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenGenerator;
        private readonly ILoggingService _logger;

        public AuthenticationAppService(
            IUserService userService,
            IPasswordService passwordService,
            ITokenService tokenGenerator,
            ILoggingService logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("O email é obrigatório.", nameof(email));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("A senha é obrigatória.", nameof(password));

            try
            {
                await _logger.LogInformationAsync("Login iniciado para o email {Email}", new object[] { email });

                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return new AuthResult { Success = false, Error = "Credenciais inválidas" };
                }

                var (hash, salt) = _passwordService.SplitPasswordHash(user.PasswordHash);
                if (!_passwordService.VerifyPassword(password, hash, salt))
                {
                    return new AuthResult { Success = false, Error = "Credenciais inválidas" };
                }

                var token = _tokenGenerator.GenerateJwtToken(user);
                var refreshToken = _tokenGenerator.GenerateRefreshToken();

                user.RefreshTokens.Add(new RefreshToken
                {
                    Token = refreshToken,
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
                });
                user.LastLogin = DateTimeOffset.UtcNow;

                await _userService.UpdateUserAsync(user);
                await _logger.LogInformationAsync("Login bem-sucedido para o email {Email}", new object[] { email });

                return new AuthResult
                {
                    Success = true,
                    Token = "Bearer " + token,
                    RefreshToken = refreshToken,
                    UserId = user.Id,
                    UserEmail = user.Email
                };
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro durante o login para o email {Email}", new object[] { email });
                throw new ApplicationException("Erro durante o login. Tente novamente mais tarde.", ex);
            }
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("O refresh token é obrigatório.", nameof(refreshToken));

            try
            {
                await _logger.LogInformationAsync("Iniciando renovação de sessão com refreshToken {RefreshToken}", new object[] { refreshToken });

                var user = await _userService.GetUserByRefreshTokenAsync(refreshToken);
                if (user == null)
                {
                    return new AuthResult { Success = false, Error = "Invalid refresh token" };
                }

                var existingToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);
                if (existingToken == null || existingToken.ExpiresAt <= DateTimeOffset.UtcNow)
                {
                    return new AuthResult { Success = false, Error = "Refresh token inválido ou expirado" };
                }

                var newToken = _tokenGenerator.GenerateJwtToken(user);
                var newRefreshToken = _tokenGenerator.GenerateRefreshToken();

                user.RefreshTokens.Remove(existingToken);
                user.RefreshTokens.Add(new RefreshToken
                {
                    Token = newRefreshToken,
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
                });

                await _userService.UpdateUserAsync(user);
                await _logger.LogInformationAsync("Renovação de sessão bem-sucedida para o refreshToken {RefreshToken}", new object[] { refreshToken });

                return new AuthResult
                {
                    Success = true,
                    Token = "Bearer " + newToken,
                    RefreshToken = newRefreshToken
                };
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro ao renovar sessão com refreshToken {RefreshToken}", new object[] { refreshToken });
                throw new ApplicationException("Erro ao renovar sessão. Tente novamente mais tarde.", ex);
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("O refresh token é obrigatório.", nameof(refreshToken));

            try
            {
                var user = await _userService.GetUserByRefreshTokenAsync(refreshToken);
                if (user == null)
                    return false;

                var token = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);
                if (token == null)
                    return false;

                await _logger.LogInformationAsync("Iniciando logout para o refreshToken {RefreshToken}", new object[] { refreshToken });
                
                user.RefreshTokens.Remove(token);
                await _userService.UpdateUserAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "Erro durante o logout com refreshToken {RefreshToken}", new object[] { refreshToken });
                throw new ApplicationException("Erro durante o logout. Tente novamente mais tarde.", ex);
            }
        }
    }
}
