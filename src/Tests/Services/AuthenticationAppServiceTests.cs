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
    public class AuthenticationAppServiceTests
    {
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<IPasswordService> _passwordServiceMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<ILoggingService> _loggerMock = new();
        private readonly AuthenticationAppService _service;

        public AuthenticationAppServiceTests()
        {
            _service = new AuthenticationAppService(
                _userServiceMock.Object,
                _passwordServiceMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_ReturnsError_WhenInvalidCredentials()
        {
            _userServiceMock.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var result = await _service.LoginAsync("invalid@test.com", "wrongpass");

            Assert.False(result.Success);
        }

        [Fact]
        public async Task RefreshTokenAsync_GeneratesNewTokens()
        {
            var user = new User { RefreshTokens = new List<RefreshToken> { new RefreshToken { Token = "old", ExpiresAt = DateTimeOffset.Now.AddDays(1) } } };
            _userServiceMock.Setup(x => x.GetUserByRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync(user);

            var result = await _service.RefreshTokenAsync("old");

            Assert.NotEqual("old", result.RefreshToken);
        }
    }
}