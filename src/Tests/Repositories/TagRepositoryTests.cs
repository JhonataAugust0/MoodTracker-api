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

namespace MoodTracker_back.Tests
{
    public class TagRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly TagRepository _repository;
        private readonly Mock<ILoggingService> _loggerMock;

        public TagRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _loggerMock = new Mock<ILoggingService>();

            _repository = new TagRepository(_context, _loggerMock.Object);

            SeedData();
        }

        private void SeedData()
        {
            var tags = new List<Tag>
            {
                new Tag { Id = 1, UserId = 1, CreatedAt = DateTime.UtcNow.AddHours(-1) },
                new Tag { Id = 2, UserId = 1, CreatedAt = DateTime.UtcNow.AddHours(-2) },
                new Tag { Id = 3, UserId = 2, CreatedAt = DateTime.UtcNow.AddHours(-3) }
            };

            _context.Tags.AddRange(tags);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetByIdAsync_TagExistente_RetornaTag()
        {
            var tag = await _repository.GetByIdAsync(1);

            Assert.NotNull(tag);
            Assert.Equal(1, tag.Id);
        }

        [Fact]
        public async Task GetByIdAsync_TagInexistente_RetornaNull()
        {
            var tag = await _repository.GetByIdAsync(999);

            Assert.Null(tag);
        }

        [Fact]
        public async Task GetByUserIdAsync_RetornaTagsOrdenadasDesc()
        {
            var tags = (await _repository.GetUserTagsAsync(1)).ToList();
            Assert.Equal(2, tags.Count);

            Assert.True(tags[0].CreatedAt >= tags[1].CreatedAt);
        }

        [Fact]
        public async Task GetUserTagsAsync_RetornaTagsDoUsuarioSemOrdenacao()
        {
            var tags = (await _repository.GetUserTagsAsync(1)).ToList();

            Assert.Equal(2, tags.Count);
        }

        [Fact]
        public async Task GetByIdsAsync_ComIdsValidos_RetornaTagsCorretas()
        {
            var ids = new List<int> { 1, 3 };

            var tags = (await _repository.GetByIdsAsync(ids)).ToList();

            Assert.Equal(2, tags.Count);
            Assert.Contains(tags, t => t.Id == 1);
            Assert.Contains(tags, t => t.Id == 3);
        }

        [Fact]
        public async Task GetByIdsAsync_ComIdsNulosOuVazios_RetornaListaVazia()
        {
            var tagsNull = await _repository.GetByIdsAsync(null);
            var tagsEmpty = await _repository.GetByIdsAsync(new List<int>());

            Assert.Empty(tagsNull);
            Assert.Empty(tagsEmpty);
        }

        [Fact]
        public async Task CreateAsync_NovaTag_CriadaCorretamente()
        {
            var novaTag = new Tag { Id = 10, UserId = 3, CreatedAt = DateTime.UtcNow };

            await _repository.CreateAsync(novaTag);

            var tagNoBanco = await _context.Tags.FindAsync(10);
            Assert.NotNull(tagNoBanco);
            Assert.Equal(3, tagNoBanco.UserId);

            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_TagExistente_AtualizadaCorretamente()
        {
            var tag = await _repository.GetByIdAsync(1);
            tag.CreatedAt = DateTime.UtcNow.AddMinutes(-30);

            await _repository.UpdateAsync(tag);

            var tagAtualizada = await _context.Tags.FindAsync(1);
            Assert.Equal(tag.CreatedAt, tagAtualizada.CreatedAt);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_TagNaoExistente_LancaDbUpdateConcurrencyException()
        {
            var tag = new Tag { Id = 999, UserId = 1, CreatedAt = DateTime.UtcNow };

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _repository.UpdateAsync(tag));
            _loggerMock.Verify(l => l.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task DeleteAsync_TagExistente_DeletadaCorretamente()
        {
            await _repository.DeleteAsync(2);

            var tagDeletada = await _context.Tags.FindAsync(2);
            Assert.Null(tagDeletada);
            _loggerMock.Verify(l => l.LogInformationAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_TagInexistente_ApenasLogWarningChamado()
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
