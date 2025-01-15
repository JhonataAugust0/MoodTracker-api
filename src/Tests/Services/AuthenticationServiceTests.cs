using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Adapters;
using Moq;
using Xunit;

namespace MoodTracker_back.Tests.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly AuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenGeneratorMock = new Mock<ITokenGenerator>();
        _emailServiceMock = new Mock<IEmailService>();

        _authService = new AuthenticationService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenGeneratorMock.Object,
            _emailServiceMock.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_ReturnsSuccess()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var salt = "salt123";
        var hash = "hash123";
        var jwt = "jwt123";
        var refreshToken = "refresh123";

        _userRepositoryMock.Setup(x => x.EmailExistsAsync(email))
            .ReturnsAsync(false);

        _passwordHasherMock.Setup(x => x.HashPassword(password, out salt))
            .Returns(hash);

        _tokenGeneratorMock.Setup(x => x.GenerateJwtToken(It.IsAny<User>()))
            .Returns(jwt);

        _tokenGeneratorMock.Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);

        // Act
        var result = await _authService.RegisterAsync(email, password, null);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(jwt, result.Token);
        Assert.Equal(refreshToken, result.RefreshToken);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ReturnsFailure()
    {
        // Arrange
        var email = "existing@example.com";
        _userRepositoryMock.Setup(x => x.EmailExistsAsync(email))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterAsync(email, "password", null);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Email already exists", result.Error);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var storedHash = "hash123:salt123";
        var jwt = "jwt123";
        var refreshToken = "refresh123";

        var user = new User { Email = email, PasswordHash = storedHash };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.VerifyPassword(password, "hash123", "salt123"))
            .Returns(true);

        _tokenGeneratorMock.Setup(x => x.GenerateJwtToken(user))
            .Returns(jwt);

        _tokenGeneratorMock.Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(jwt, result.Token);
        Assert.Equal(refreshToken, result.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "wrongpassword";
        var storedHash = "hash123:salt123";

        var user = new User { Email = email, PasswordHash = storedHash };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.VerifyPassword(password, "hash123", "salt123"))
            .Returns(false);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid credentials", result.Error);
    }
}