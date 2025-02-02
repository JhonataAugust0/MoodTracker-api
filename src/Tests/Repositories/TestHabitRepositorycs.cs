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
    public class HabitRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly HabitRepository _repository;
        private readonly Mock<ILoggingService> _loggerMock;

        public HabitRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _loggerMock = new Mock<ILoggingService>();

            _repository = new HabitRepository(_context, _loggerMock.Object);

            SeedData();
        }

        private void SeedData()
        {
            var habits = new List<Habit>
            {
                new Habit { Id = 1, UserId = 1, Color = "#fffff",CreatedAt = DateTime.UtcNow.AddDays(-2), Tags = new List<Tag>() },
                new Habit { Id = 2, UserId = 1, Color = "#fffff",CreatedAt = DateTime.UtcNow.AddDays(-1), Tags = new List<Tag>() },
                new Habit { Id = 3, UserId = 2, Color = "#fffff",CreatedAt = DateTime.UtcNow.AddDays(-3), Tags = new List<Tag>() }
            };

            _context.Habits.AddRange(habits);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetByIdAsync_ExistingHabit_ReturnsHabit()
        {
            var habit = await _repository.GetByIdAsync(1);

            Assert.NotNull(habit);
            Assert.Equal(1, habit.Id);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingHabit_ReturnsNull()
        {
            var habit = await _repository.GetByIdAsync(999);

            Assert.Null(habit);
        }

        [Fact]
        public async Task GetUserHabitsAsync_ReturnsHabitsOrderedByIdDescending()
        {
            var habits = (await _repository.GetUserHabitsAsync(1)).ToList();

            Assert.Equal(2, habits.Count);
            Assert.True(habits[0].Id >= habits[1].Id);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsHabitsOrderedByCreatedAtDescending()
        {
            var habits = (await _repository.GetByUserIdAsync(1)).ToList();

            Assert.Equal(2, habits.Count);
            Assert.True(habits[0].CreatedAt >= habits[1].CreatedAt);
        }

        [Fact]
        public async Task CreateAsync_NewHabit_CreatesHabitAndLogsInformation()
        {
            var newHabit = new Habit
            {
                Id = 10,
                UserId = 3,
                CreatedAt = DateTime.UtcNow,
                Tags = new List<Tag>(),
                Color = "#FFFFF"
            };

            await _repository.CreateAsync(newHabit);

            var habitFromDb = await _context.Habits.FindAsync(10);
            Assert.NotNull(habitFromDb);
            Assert.Equal(3, habitFromDb.UserId);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingHabit_UpdatesHabitAndLogsInformation()
        {
            var habit = await _repository.GetByIdAsync(1);
            var newCreatedAt = DateTime.UtcNow;
            habit.CreatedAt = newCreatedAt;

            await _repository.UpdateAsync(habit);

            var updatedHabit = await _context.Habits.FindAsync(1);
            Assert.Equal(newCreatedAt, updatedHabit.CreatedAt);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingHabit_ThrowsDbUpdateConcurrencyException()
        {
            var habit = new Habit { Id = 999, UserId = 1, CreatedAt = DateTime.UtcNow, Tags = new List<Tag>() };

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _repository.UpdateAsync(habit));
            _loggerMock.Verify(l => l.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task DeleteAsync_ExistingHabit_DeletesHabitAndLogsInformation()
        {
            await _repository.DeleteAsync(2);

            var deletedHabit = await _context.Habits.FindAsync(2);
            Assert.Null(deletedHabit);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingHabit_LogsWarning()
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
