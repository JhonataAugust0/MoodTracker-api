using Infrastructure.Adapters;
using Xunit;

namespace MoodTracker_back.Tests.Adapters;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_ReturnsDifferentHashesForSamePassword()
    {
        // Arrange
        var password = "password123";

        // Act
        var hash1 = _passwordHasher.HashPassword(password, out string salt1);
        var hash2 = _passwordHasher.HashPassword(password, out string salt2);

        // Assert
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(salt1, salt2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "password123";
        var hash = _passwordHasher.HashPassword(password, out string salt);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash, salt);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "password123";
        var hash = _passwordHasher.HashPassword(password, out string salt);

        // Act
        var result = _passwordHasher.VerifyPassword("wrongpassword", hash, salt);

        // Assert
        Assert.False(result);
    }
}
