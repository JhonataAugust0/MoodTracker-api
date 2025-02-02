using Moq;
using Xunit;
using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Domain.Exceptions;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Tests.Services
{
    public class TagAppServiceTests
    {
        private readonly Mock<ITagRepository> _tagRepoMock = new();
        private readonly Mock<ILoggingService> _loggerMock = new();
        private readonly TagAppService _service;

        public TagAppServiceTests()
        {
            _service = new TagAppService(_tagRepoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetTagByIdAsync_ReturnsTag_WhenExistsAndOwned()
        {
            
            var tag = new Tag { Id = 1, UserId = 1 };
            _tagRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(tag);

            var result = await _service.GetTagByIdAsync(1, 1);

            Assert.Equal(tag.Id, result.Id);
        }

        [Fact]
        public async Task GetTagByIdAsync_Throws_WhenNotOwned()
        {
            
            var tag = new Tag { Id = 1, UserId = 2 };
            _tagRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(tag);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetTagByIdAsync(1, 1));
        }

        [Fact]
        public async Task CreateTagAsync_Throws_WhenNameEmpty()
        {
            
            var dto = new CreateTagDto() { Name = "" };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTagAsync(1, dto));
        }

        [Fact]
        public async Task DeleteTagAsync_Deletes_WhenValid()
        {
            
            var tag = new Tag { Id = 1, UserId = 1 };
            _tagRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(tag);

            await _service.DeleteTagAsync(1, 1);

            _tagRepoMock.Verify(x => x.DeleteAsync(1), Times.Once);
        }
    }
}