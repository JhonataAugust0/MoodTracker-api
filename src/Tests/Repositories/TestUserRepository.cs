using Moq;
using Xunit;
using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Infrastructure.Data.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Infrastructure.Data.Postgres.Config;

namespace MoodTracker_back.Tests
{
    public class TestUserRepository : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserRepository _repository;
        private readonly Mock<ILoggingService> _mockLogger;

        public TestUserRepository()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _mockLogger = new Mock<ILoggingService>();
            _repository = new UserRepository(_context, _mockLogger.Object);
            SeedData();
        }

        private void SeedData()
        {
            var users = new List<User>
            {
                new User 
                { 
                    Id = 1, 
                    Email = "teste@example.com", 
                    Name = "teste", 
                    Preferences = {}, 
                    IsActive = true, 
                    PasswordHash = "hash1", 
                    LastMoodEntry = DateTime.UtcNow.AddHours(-1), 
                    CreatedAt = DateTime.UtcNow.AddHours(-1) 
                },
                new User 
                { 
                    Id = 2, 
                    Email = "joao@example.com", 
                    Name = "joao", 
                    Preferences = {}, 
                    IsActive = true, 
                    PasswordHash = "hash2", 
                    LastMoodEntry = DateTime.UtcNow.AddHours(-1), 
                    CreatedAt = DateTime.UtcNow.AddHours(-2) 
                },
                new User 
                { 
                    Id = 3, 
                    Email = "cleiton@example.com", 
                    Name = "cleiton", 
                    Preferences = {}, 
                    IsActive = true, 
                    PasswordHash = "hash3", 
                    LastMoodEntry = DateTime.UtcNow.AddHours(-1), 
                    CreatedAt = DateTime.UtcNow.AddHours(-3) 
                }
            };

            _context.Users.AddRange(users);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetByEmailAsync_ReturnsUser_WhenEmailExists()
        {
            var user = await _repository.GetByEmailAsync("teste@example.com");

            Assert.NotNull(user);
            Assert.Equal(1, user!.Id);
        }

        [Fact]
        public async Task GetByEmailAsync_ThrowsArgumentException_WhenEmailIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetByEmailAsync(string.Empty));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsUser_WhenExists()
        {
            var user = await _repository.GetByIdAsync(2);

            Assert.NotNull(user);
            Assert.Equal("joao@example.com", user!.Email);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_ReturnsUser_WhenTokenExists()
        {
            var user = await _repository.GetByIdAsync(1);
            var refreshToken = new RefreshToken { Token = "test-token", ExpiresAt = DateTime.UtcNow.AddDays(7) };
            user!.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            var foundUser = await _repository.GetUserByRefreshTokenAsync("test-token");

            Assert.NotNull(foundUser);
            Assert.Equal(1, foundUser!.Id);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_ThrowsArgumentException_WhenTokenIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetUserByRefreshTokenAsync(string.Empty));
        }

        [Fact]
        public async Task GetInactiveUsers_ReturnsInactiveUsers()
        {
            var user = await _repository.GetByIdAsync(1);
            user!.LastMoodEntry = DateTime.UtcNow.AddDays(-5);
            await _context.SaveChangesAsync();

            var inactiveUsers = (await _repository.GetInactiveUsers(CancellationToken.None)).ToList();

            Assert.NotEmpty(inactiveUsers);
            Assert.Contains(inactiveUsers, u => u.Id == 1);
        }

        [Fact]
        public async Task UpdateLastNotifiedAsync_UpdatesUser_WhenUserExists()
        {
            var now = DateTime.UtcNow;

            await _repository.UpdateLastNotifiedAsync(2, now, CancellationToken.None);
            var user = await _repository.GetByIdAsync(2);

            Assert.NotNull(user);
            Assert.Equal(now, user!.LastNotified);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesUser_WhenUserExists()
        {
            var user = await _repository.GetByIdAsync(3);
            user!.Name = "updatedName";

            await _repository.UpdateAsync(user);
            var updatedUser = await _repository.GetByIdAsync(3);

            Assert.Equal("updatedName", updatedUser!.Name);
        }

        [Fact]
        public async Task EmailExistsAsync_ReturnsTrue_WhenEmailExists()
        {
            var exists = await _repository.EmailExistsAsync("teste@example.com");

            Assert.True(exists);
        }

        [Fact]
        public async Task EmailExistsAsync_ReturnsFalse_WhenEmailDoesNotExist()
        {
            var exists = await _repository.EmailExistsAsync("naoexiste@example.com");

            Assert.False(exists);
        }

        [Fact]
        public async Task DeleteAsync_RemovesUser_WhenUserExists()
        {
            var user = await _repository.GetByIdAsync(3);

            await _repository.DeleteAsync(user!);
            var deletedUser = await _repository.GetByIdAsync(3);

            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task CreateAsync_CreatesUser_AndLogsInformation()
        {
            var newUser = new User
            {
                Id = 4,
                Email = "teste1@example.com",
                Name = "teste1",
                Preferences = {},
                IsActive = true,
                PasswordHash = "hash4",
                LastMoodEntry = DateTime.UtcNow.AddHours(-1),
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            };

            await _repository.CreateAsync(newUser);
            var createdUser = await _repository.GetByIdAsync(4);

            Assert.NotNull(createdUser);
            Assert.Equal(4, createdUser!.Id);
            _mockLogger.Verify(l => l.LogInformationAsync(
                It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ThrowsArgumentNullException_WhenUserIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.CreateAsync(null!));
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllUsers()
        {
            var users = await _repository.GetAllAsync();

            Assert.Equal(3, users.Count());
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}