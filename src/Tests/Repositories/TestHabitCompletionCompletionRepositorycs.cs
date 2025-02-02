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
    public class HabitCompletionCompletionRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly HabitCompletionCompletionRepository _repository;
        private readonly Mock<ILoggingService> _loggerMock;

        public HabitCompletionCompletionRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _loggerMock = new Mock<ILoggingService>();

            _repository = new HabitCompletionCompletionRepository(_context, _loggerMock.Object);

            SeedData();
        }

        private void SeedData()
        {
            var completions = new List<HabitCompletion>
            {
                new HabitCompletion 
                { 
                    Id = 1, 
                    HabitId = 1, 
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-3), 
                    CompletedAt = DateTimeOffset.UtcNow.AddDays(-2) 
                },
                new HabitCompletion 
                { 
                    Id = 2, 
                    HabitId = 1, 
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-2), 
                    CompletedAt = DateTimeOffset.UtcNow.AddDays(-1) 
                },
                new HabitCompletion 
                { 
                    Id = 3, 
                    HabitId = 2, 
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-4), 
                    CompletedAt = DateTimeOffset.UtcNow.AddDays(-3) 
                }
            };

            _context.HabitCompletions.AddRange(completions);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetUserHistoryHabitCompletionAsync_WithDateFilters_ReturnsFilteredCompletions()
        {
            int habitId = 1;
            DateTimeOffset startDate = DateTimeOffset.UtcNow.AddDays(-3);
            DateTimeOffset endDate = DateTimeOffset.UtcNow.AddDays(0);

            var completions = (await _repository.GetUserHistoryHabitCompletionAsync(habitId, startDate, endDate)).ToList();

            Assert.NotEmpty(completions);
            Assert.All(completions, c => Assert.Equal(habitId, c.HabitId));

            Assert.True(completions.First().CompletedAt >= completions.Last().CompletedAt);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingCompletion_ReturnsCompletion()
        {
            var completion = await _repository.GetByIdAsync(1);

            Assert.NotNull(completion);
            Assert.Equal(1, completion.Id);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingCompletion_ReturnsNull()
        {
            var completion = await _repository.GetByIdAsync(999);

            Assert.Null(completion);
        }

        [Fact]
        public async Task GetByHabitIdAsync_ReturnsCompletionsOrderedByCreatedAtDescending()
        {
            var completions = (await _repository.GetByHabitIdAsync(1)).ToList();

            Assert.NotEmpty(completions);
            Assert.All(completions, c => Assert.Equal(1, c.HabitId));
            Assert.True(completions.First().CreatedAt >= completions.Last().CreatedAt);
        }

        [Fact]
        public async Task CreateAsync_NewCompletion_CreatesCompletionAndLogsInformation()
        {
            var newCompletion = new HabitCompletion
            {
                Id = 10,
                HabitId = 3,
                CreatedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow.AddHours(1)
            };

            await _repository.CreateAsync(newCompletion);

            var completionFromDb = await _context.HabitCompletions.FindAsync(10);
            Assert.NotNull(completionFromDb);
            Assert.Equal(3, completionFromDb.HabitId);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task UpdateAsync_ExistingCompletion_UpdatesCompletionAndLogsInformation()
        {
            var completion = await _repository.GetByIdAsync(1);
            var newCompletedAt = DateTimeOffset.UtcNow.AddMinutes(30);
            completion.CompletedAt = newCompletedAt;

            await _repository.UpdateAsync(completion);

            var updatedCompletion = await _context.HabitCompletions.FindAsync(1);
            Assert.Equal(newCompletedAt, updatedCompletion.CompletedAt);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingCompletion_ThrowsDbUpdateConcurrencyException()
        {
            var completion = new HabitCompletion { Id = 999, HabitId = 1, CreatedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow };

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _repository.UpdateAsync(completion));
            _loggerMock.Verify(l => l.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task DeleteAsync_ExistingCompletion_DeletesCompletionAndLogsInformation()
        {
            await _repository.DeleteAsync(2);

            var deletedCompletion = await _context.HabitCompletions.FindAsync(2);
            Assert.Null(deletedCompletion);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingCompletion_LogsWarning()
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
