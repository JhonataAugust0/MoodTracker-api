using Moq;
using Xunit;
using MoodTracker_back.Domain.Entities;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Application.Services;
using MoodTracker_back.Domain.Exceptions;
using MoodTracker_back.Domain.Interfaces;

namespace MoodTracker_back.Tests.Services
{
    public class UserAppServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IPasswordService> _passwordServiceMock = new();
        private readonly Mock<ILoggingService> _loggerMock = new();
        private readonly UserAppService _service;

        public UserAppServiceTests()
        {
            _service = new UserAppService(
                _userRepositoryMock.Object,
                _passwordServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task RegisterUserAsync_ThrowsWhenEmailOrPasswordEmpty()
        {
            
            var email = "";
            var password = "password";

            await Assert.ThrowsAsync<ArgumentException>(() => _service.RegisterUserAsync(email, password, null));
            await Assert.ThrowsAsync<ArgumentException>(() => _service.RegisterUserAsync("test@test.com", "", null));
        }

        [Fact]
        public async Task RegisterUserAsync_ThrowsWhenEmailExists()
        {
            
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<ApplicationException>(() => 
                _service.RegisterUserAsync("existing@test.com", "password", "teste"));
        }

        [Fact]
        public async Task RegisterUserAsync_CreatesUserWhenValid()
        {
            
            var expectedUser = new User { Email = "test@test.com" };
            _userRepositoryMock.Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            _passwordServiceMock.Setup(x => x.HashPassword(It.IsAny<string>(), out It.Ref<string>.IsAny))
                .Returns("hash");

            var result = await _service.RegisterUserAsync("test@test.com", "password", null);

            Assert.NotNull(result);
            Assert.Contains("hash", result.PasswordHash);
            _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUserWhenExists()
        {
            
            var expectedUser = new User { Id = 1 };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(expectedUser);

            var result = await _service.GetUserByIdAsync(1);

            Assert.Equal(expectedUser, result);
        }

        [Fact]
        public async Task GetUserByIdAsync_ThrowsWhenUserNotFound()
        {
            
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((User)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetUserByIdAsync(1));
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUserWhenExists()
        {
            
            var expectedUser = new User { Email = "test@test.com" };
            _userRepositoryMock.Setup(x => x.GetByEmailAsync("test@test.com"))
                .ReturnsAsync(expectedUser);

            var result = await _service.GetUserByEmailAsync("test@test.com");

            Assert.Equal(expectedUser, result);
        }

        [Fact]
        public async Task EmailExistsAsync_ReturnsTrueWhenEmailExists()
        {
            
            _userRepositoryMock.Setup(x => x.EmailExistsAsync("test@test.com"))
                .ReturnsAsync(true);

            var result = await _service.EmailExistsAsync("test@test.com");

            Assert.True(result);
        }

        // [Fact]
        // public async Task GetUserByRefreshTokenAsync_ReturnsUserWhenValidToken()
        // {
        //     
        //     var expectedUser = new User { RefreshTokens = "valid_token" };
        //     _userRepositoryMock.Setup(x => x.GetUserByRefreshTokenAsync("valid_token"))
        //         .ReturnsAsync(expectedUser);
        //
        //     // Act
        //     var result = await _service.GetUserByRefreshTokenAsync("valid_token");
        //
        //     // Assert
        //     Assert.Equal(expectedUser, result);
        // }

        [Fact]
        public async Task GetInactiveUsers_ReturnsUsersFromRepository()
        {
            
            var expectedUsers = new List<User> { new User() };
            _userRepositoryMock.Setup(x => x.GetInactiveUsers(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUsers);

            var result = await _service.GetInactiveUsers(CancellationToken.None);

            Assert.Equal(expectedUsers, result);
        }

        [Fact]
        public async Task UpdateUserLastNotifiedAsync_UpdatesRepository()
        {
            
            var lastNotified = DateTime.UtcNow;
            _userRepositoryMock.Setup(x => x.UpdateLastNotifiedAsync(1, lastNotified, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _service.UpdateUserLastNotifiedAsync(1, lastNotified, CancellationToken.None);

            _userRepositoryMock.Verify(x => x.UpdateLastNotifiedAsync(1, lastNotified, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsWhenUserNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateUserAsync(null));
        }

        [Fact]
        public async Task DeleteUserAsync_DeletesWhenUserExists()
        {
            
            var user = new User { Id = 1 };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            await _service.DeleteUserAsync(1);

            _userRepositoryMock.Verify(x => x.DeleteAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_ThrowsWhenUserNotFound()
        {
            
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((User)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteUserAsync(1));
        }

        [Fact]
        public async Task AllMethods_LogErrorsOnExceptions()
        {
            
            var exception = new Exception("Test error");
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ThrowsAsync(exception);

            await Assert.ThrowsAsync<ApplicationException>(() => _service.GetUserByIdAsync(1));
            _loggerMock.Verify(x => x.LogErrorAsync(exception, It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}