using Moq;
using Xunit;
using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Domain.Exceptions;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Tests.Services
{
    public class HabitAppServiceTests
    {
        private readonly Mock<IHabitRepository> _habitRepoMock = new();
        private readonly Mock<IHabitCompletionRepository> _completionRepoMock = new();
        private readonly Mock<ITagRepository> _tagRepoMock = new();
        private readonly Mock<ILoggingService> _loggerMock = new();
        private readonly HabitAppService _service;

        public HabitAppServiceTests()
        {
            _service = new HabitAppService(_habitRepoMock.Object, _tagRepoMock.Object, 
                _completionRepoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task LogHabitAsync_CreatesValidCompletion()
        {
            var habit = new Habit { Id = 1, UserId = 1 };
            _habitRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(habit);

            await _service.LogHabitAsync(1, new LogHabitCompletionDto() { HabitId = 1 });

            _completionRepoMock.Verify(x => x.CreateAsync(It.IsAny<HabitCompletion>()), Times.Once);
        }

        [Fact]
        public async Task UpdateHabitAsync_ValidatesOwnership()
        {
            var habit = new Habit { Id = 1, UserId = 2 };
            _habitRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(habit);

            await Assert.ThrowsAsync<NotFoundException>(() => 
                _service.UpdateHabitAsync(1, 1, new UpdateHabitDto()));
        }
    }
}