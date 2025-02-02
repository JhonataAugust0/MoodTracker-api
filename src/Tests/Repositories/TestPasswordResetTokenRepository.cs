using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoodTracker_back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Infrastructure.Data.Postgres.Config;
using MoodTracker_back.Infrastructure.Data.Postgres.Repositories;
using Xunit;

namespace MoodTracker_back.Tests.Repositories
{
    public class PasswordResetTokenRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordResetTokenRepository _repository;
        private readonly Mock<ILoggingService> _loggerMock;

        public PasswordResetTokenRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _loggerMock = new Mock<ILoggingService>();

            _repository = new PasswordResetTokenRepository(_context, _loggerMock.Object);

            SeedData();
        }

        private void SeedData()
        {
            var tokens = new List<PasswordResetToken>
            {
                new PasswordResetToken
                {
                    Id = 1,
                    UserId = 1,
                    Token = "token_valido",
                    Used = false,
                    ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
                },
                new PasswordResetToken
                {
                    Id = 2,
                    UserId = 1,
                    Token = "token_expirado",
                    Used = false,
                    ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1)
                },
                new PasswordResetToken
                {
                    Id = 3,
                    UserId = 2,
                    Token = "token_usado",
                    Used = true,
                    ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
                }
            };

            _context.PasswordResetTokens.AddRange(tokens);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetValidTokenAsync_ValidToken_ReturnsToken()
        {
            var token = await _repository.GetValidTokenAsync(1, "token_valido");

            Assert.NotNull(token);
            Assert.Equal("token_valido", token.Token);
        }

        [Fact]
        public async Task GetValidTokenAsync_InvalidOrExpiredToken_ReturnsNull()
        {
            var expired = await _repository.GetValidTokenAsync(1, "token_expirado");
            Assert.Null(expired);

            var used = await _repository.GetValidTokenAsync(2, "token_usado");
            Assert.Null(used);
        }

        [Fact]
        public async Task GetExpiredTokensAsync_ReturnsExpiredTokens()
        {
            var expiredTokens = (await _repository.GetExpiredTokensAsync()).ToList();

            Assert.Single(expiredTokens);
            Assert.Equal("token_expirado", expiredTokens.First().Token);
        }

        [Fact]
        public async Task AddAsync_AddsTokenSuccessfully()
        {
            var newToken = new PasswordResetToken
            {
                Id = 10,
                UserId = 3,
                Token = "novo_token",
                Used = false,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(2)
            };

            await _repository.AddAsync(newToken);
            await _repository.SaveChangesAsync();

            var tokenFromDb = await _context.PasswordResetTokens.FindAsync(10);
            Assert.NotNull(tokenFromDb);
            Assert.Equal("novo_token", tokenFromDb.Token);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task MarkAsUsedAsync_MarksTokenAsUsed()
        {
            var token = await _context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == "token_valido");
            Assert.False(token.Used);

            await _repository.MarkAsUsedAsync(token);
            await _repository.SaveChangesAsync();

            var updatedToken = await _context.PasswordResetTokens.FindAsync(token.Id);
            Assert.True(updatedToken.Used);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task RemoveRangeAsync_RemovesTokensSuccessfully()
        {
            var tokensToRemove = await _repository.GetExpiredTokensAsync();
            int countBefore = _context.PasswordResetTokens.Count();

            await _repository.RemoveRangeAsync(tokensToRemove);
            await _repository.SaveChangesAsync();

            int countAfter = _context.PasswordResetTokens.Count();
            Assert.Equal(countBefore - tokensToRemove.Count(), countAfter);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SaveChangesAsync_SavesChangesSuccessfully()
        {
            var token = await _context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == "token_valido");
            token.Used = true;

            await _repository.SaveChangesAsync();

            var updatedToken = await _context.PasswordResetTokens.FindAsync(token.Id);
            Assert.True(updatedToken.Used);
            _loggerMock.Verify(l => l.LogInformationAsync(It.Is<string>(s => s.Contains("salvas")), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
