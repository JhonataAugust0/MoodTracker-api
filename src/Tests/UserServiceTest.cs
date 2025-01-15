using System.Threading.Tasks;
using Domain.Interfaces;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Infrastructure.Adapters;
using Moq;
using Xunit;

namespace MoodTracker_back.Tests
{
    public class UserServiceTest
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly UserService _userService;

        public UserServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _userService = new UserService(_userRepositoryMock.Object, _passwordServiceMock.Object);
        }

        [Fact]
        public async Task EmailExistsAsync_EmailExists_ReturnsTrue()
        {
            // Arrange
            var email = "test@example.com";
            _userRepositoryMock.Setup(repo => repo.EmailExistsAsync(email)).ReturnsAsync(true);

            // Act
            var result = await _userService.EmailExistsAsync(email);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task EmailExistsAsync_EmailDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var email = "test@example.com";
            _userRepositoryMock.Setup(repo => repo.EmailExistsAsync(email)).ReturnsAsync(false);

            // Act
            var result = await _userService.EmailExistsAsync(email);

            // Assert
            Assert.False(result);
        }
    }
}