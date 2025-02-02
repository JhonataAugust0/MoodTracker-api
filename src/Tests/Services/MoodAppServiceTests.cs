using Moq;
using Xunit;
using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Application.Interfaces;
using System;
using System.Threading.Tasks;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Tests.Services
{
    public class MoodAppServiceTests
    {
        private readonly Mock<IMoodRepository> _moodRepoMock = new();
        private readonly Mock<ITagRepository> _tagRepoMock = new();
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<ILoggingService> _loggerMock = new();
        private readonly MoodAppService _service;

        public MoodAppServiceTests()
        {
            _service = new MoodAppService(_moodRepoMock.Object, _tagRepoMock.Object, 
                _userServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateMoodAsync_UpdatesUserTimestamps()
        {
            var user = new User { Id = 1 };
            _userServiceMock.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(user);

            await _service.CreateMoodAsync(1, new CreateMoodDto());

            Assert.NotNull(user.LastMoodEntry);
            _userServiceMock.Verify(x => x.UpdateUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task GetUserMoodHistoryAsync_ValidatesDateRange()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.GetUserMoodHistoryAsync(1, DateTimeOffset.Now, DateTimeOffset.Now.AddDays(-1)));
        }
    }
}