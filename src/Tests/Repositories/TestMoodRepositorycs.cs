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
    public class MoodRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly MoodRepository _repository;
        private readonly Mock<ILoggingService> _loggerMock;

        public MoodRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _loggerMock = new Mock<ILoggingService>();

            _repository = new MoodRepository(_context, _loggerMock.Object);

            SeedData();
        }

        private void SeedData()
        {
            var moods = new List<Mood>
            {
                new Mood { Id = 1, UserId = 1, Timestamp = DateTimeOffset.UtcNow.AddDays(-1), Tags = new List<Tag>() },
                new Mood { Id = 2, UserId = 1, Timestamp = DateTimeOffset.UtcNow, Tags = new List<Tag>() },
                new Mood { Id = 3, UserId = 2, Timestamp = DateTimeOffset.UtcNow.AddDays(-2), Tags = new List<Tag>() }
            };

            _context.MoodEntries.AddRange(moods);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetByIdAsync_ExistingMood_ReturnsMood()
        {
            var mood = await _repository.GetByIdAsync(1);

            Assert.NotNull(mood);
            Assert.Equal(1, mood.Id);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingMood_ReturnsNull()
        {
            var mood = await _repository.GetByIdAsync(999);

            Assert.Null(mood);
        }

        [Fact]
        public async Task GetUserMoodsAsync_ReturnsMoodsOrderedByIdDescending()
        {
            var moods = (await _repository.GetUserMoodsAsync(1)).ToList();

            Assert.Equal(2, moods.Count);
            Assert.True(moods[0].Id >= moods[1].Id);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsMoodsOrderedByTimestampDescending()
        {
            var moods = (await _repository.GetByUserIdAsync(1)).ToList();

            Assert.Equal(2, moods.Count);
            Assert.True(moods[0].Timestamp >= moods[1].Timestamp);
        }

        [Fact]
        public async Task GetUserHistoryMoodAsync_WithDateFilters_ReturnsFilteredMoods()
        {
            DateTimeOffset startDate = DateTimeOffset.UtcNow.AddDays(-2);
            DateTimeOffset endDate = DateTimeOffset.UtcNow.AddDays(1);

            var moods = (await _repository.GetUserHistoryMoodAsync(2, startDate, endDate)).ToList();

            Assert.Single(moods);
            Assert.Equal(2, moods.First().Id);
        }

        [Fact]
        public async Task CreateAsync_NewMood_CreatesMoodAndLogsInformation()
        {
            var newMood = new Mood
            {
                Id = 10,
                UserId = 3,
                Timestamp = DateTimeOffset.UtcNow,
                Tags = new List<Tag>()
            };

            await _repository.CreateAsync(newMood);

            var moodFromDb = await _context.MoodEntries.FindAsync(10);
            Assert.NotNull(moodFromDb);
            Assert.Equal(3, moodFromDb.UserId);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingMood_UpdatesMoodAndLogsInformation()
        {
            var mood = await _repository.GetByIdAsync(1);
            var newTimestamp = DateTimeOffset.UtcNow;
            mood.Timestamp = newTimestamp;

            await _repository.UpdateAsync(mood);

            var updatedMood = await _context.MoodEntries.FindAsync(1);
            Assert.Equal(newTimestamp, updatedMood.Timestamp);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingMood_ThrowsDbUpdateConcurrencyException()
        {
            var mood = new Mood { Id = 999, UserId = 1, Timestamp = DateTimeOffset.UtcNow, Tags = new List<Tag>() };

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _repository.UpdateAsync(mood));
            _loggerMock.Verify(l => l.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task DeleteAsync_ExistingMood_DeletesMoodAndLogsInformation()
        {
            await _repository.DeleteAsync(2);

            var deletedMood = await _context.MoodEntries.FindAsync(2);
            Assert.Null(deletedMood);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingMood_LogsWarning()
        {
            await _repository.DeleteAsync(999);

            _loggerMock.Verify(l => l.LogWarningAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
