using MoodTracker_back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Infrastructure.Data.Postgres.Config;
using MoodTracker_back.Infrastructure.Data.Postgres.Repositories;
using Xunit;

namespace MoodTracker_back.Tests.Repositories
{
    public class QuickNotesRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly QuickNotesRepository _repository;
        private readonly Mock<ILoggingService> _loggerMock;

        public QuickNotesRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _loggerMock = new Mock<ILoggingService>();

            _repository = new QuickNotesRepository(_context, _loggerMock.Object);

            SeedData();
        }

        private void SeedData()
        {
            var notes = new List<QuickNote>
            {
                new QuickNote { Id = 1, UserId = 1, CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
                new QuickNote { Id = 2, UserId = 1, CreatedAt = DateTime.UtcNow.AddMinutes(-20) },
                new QuickNote { Id = 3, UserId = 2, CreatedAt = DateTime.UtcNow.AddMinutes(-30) }
            };

            _context.QuickNotes.AddRange(notes);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetByIdAsync_ExistingNote_ReturnsNote()
        {
            var note = await _repository.GetByIdAsync(1);
            Assert.NotNull(note);
            Assert.Equal(1, note.Id);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingNote_ReturnsNull()
        {
            var note = await _repository.GetByIdAsync(999);

            Assert.Null(note);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsNotesOrderedByCreatedAtDescending()
        {
            var notes = (await _repository.GetByUserIdAsync(1)).ToList();

            Assert.Equal(2, notes.Count);
            Assert.True(notes[0].CreatedAt >= notes[1].CreatedAt);
        }

        [Fact]
        public async Task CreateAsync_NewNote_CreatesNote()
        {
            var newNote = new QuickNote { Id = 10, UserId = 3, CreatedAt = DateTime.UtcNow };

            await _repository.CreateAsync(newNote);

            var createdNote = await _context.QuickNotes.FindAsync(10);
            Assert.NotNull(createdNote);
            Assert.Equal(3, createdNote.UserId);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingNote_UpdatesNote()
        {
            var note = await _repository.GetByIdAsync(1);
            var newCreatedAt = DateTime.UtcNow.AddMinutes(-5);
            note.CreatedAt = newCreatedAt;

            await _repository.UpdateAsync(note);

            var updatedNote = await _context.QuickNotes.FindAsync(1);
            Assert.Equal(newCreatedAt, updatedNote.CreatedAt);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingNote_ThrowsDbUpdateConcurrencyException()
        {
            var note = new QuickNote { Id = 999, UserId = 1, CreatedAt = DateTime.UtcNow };

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _repository.UpdateAsync(note));
            _loggerMock.Verify(l => l.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task DeleteAsync_ExistingNote_DeletesNote()
        {
            await _repository.DeleteAsync(2);

            var note = await _context.QuickNotes.FindAsync(2);
            Assert.Null(note);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingNote_LogsWarning()
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
