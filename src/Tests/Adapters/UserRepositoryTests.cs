using Domain.Entities;
using Infrastructure.Data.Config;
using Infrastructure.Data.Repositories;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace MoodTracker_back.Tests.Adapters;

public class UserRepositoryTests : IDisposable
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _context = new ApplicationDbContext(_options);
        _repository = new UserRepository(_context);
    }

    // [Fact]
    // public async Task CreateAsync_SavesUserToDatabase()
    // {
    //     // Arrange
    //     var user = new User()
    //     {
    //         Email = "test@example.com",
    //         PasswordHash = "hash123:salt123"
    //     };
    //
    //     // Act
    //     var result = await _repository.CreateAsync(user);
    //
    //     // Assert
    //     Assert.NotEqual(0, result.Id);
    //     var savedUser = await _context.Users.FindAsync(result.Id);
    //     Assert.NotNull(savedUser);
    //     Assert.Equal(user.Email, savedUser.Email);
    // }

    [Fact]
    public async Task GetByEmailAsync_ReturnsCorrectUser()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash123:salt123"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(user.Email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task EmailExistsAsync_WithExistingEmail_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash123:salt123"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.EmailExistsAsync(user.Email);

        // Assert
        Assert.True(result);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}