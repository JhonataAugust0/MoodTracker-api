using Moq;
using Xunit;
using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoodTracker_back.Domain.Interfaces;
using MoodTracker_back.Presentation.Api.V1.Dtos;

namespace MoodTracker_back.Tests.Services
{
    public class QuickNotesAppServiceTests
    {
        private readonly Mock<IQuickNoteRepository> _noteRepoMock = new();
        private readonly Mock<ITagRepository> _tagRepoMock = new();
        private readonly Mock<ICryptographService> _cryptoServiceMock = new();
        private readonly Mock<ILoggingService> _loggerMock = new();
        private readonly QuickNotesAppService _service;

        public QuickNotesAppServiceTests()
        {
            _service = new QuickNotesAppService(_noteRepoMock.Object, _tagRepoMock.Object, _cryptoServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateNoteAsync_AddsValidTags()
        {
            
            var tags = new List<Tag> { new Tag { Id = 1, UserId = 1 } };
            _tagRepoMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(tags);

            await _service.CreateNoteAsync(1, new CreateQuickNoteDto() { 
                Content = "Test", 
                TagIds = new List<int> { 1 } 
            });

            _noteRepoMock.Verify(x => x.CreateAsync(It.Is<QuickNote>(n => n.Tags.Any())), Times.Once);
        }

        [Fact]
        public async Task UpdateNoteAsync_ReplacesTags()
        {
            
            var note = new QuickNote { UserId = 1, Tags = new List<Tag>() };
            var tags = new List<Tag> { new Tag { Id = 1, UserId = 1 } };
            _noteRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(note);
            _tagRepoMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(tags);

            await _service.UpdateNoteAsync(1, 1, new UpdateQuickNoteDto { 
                TagIds = new List<int> { 1 } 
            });

            Assert.Single(note.Tags);
        }
    }
}