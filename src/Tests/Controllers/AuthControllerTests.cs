using Application.Dtos;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using MoodTracker_back.Presentation.Api.V1.Dtos;
using MoodTracker_back.Presentation.Controllers;
using Moq;
using Xunit;

namespace MoodTracker_back.Tests.Controllers;


public class AuthControllerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthenticationService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Register_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new RegisterRequestDTO() 
        { 
            Email = "test@example.com", 
            Password = "password123" 
        };

        var authResult = new AuthResult() 
        { 
            Success = true, 
            Token = "jwt123", 
            RefreshToken = "refresh123" 
        };

        _authServiceMock.Setup(x => x.RegisterAsync(
            request.Email, request.Password, request.Name))
            .ReturnsAsync(authResult);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<AuthResult>(okResult.Value);
        Assert.True(returnValue.Success);
        Assert.Equal(authResult.Token, returnValue.Token);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkResult()
    {
        // Arrange
        var request = new LoginRequestDTO() 
        { 
            Email = "test@example.com", 
            Password = "password123" 
        };

        var authResult = new AuthResult 
        { 
            Success = true, 
            Token = "jwt123", 
            RefreshToken = "refresh123" 
        };

        _authServiceMock.Setup(x => x.LoginAsync(request.Email, request.Password))
            .ReturnsAsync(authResult);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<AuthResult>(okResult.Value);
        Assert.True(returnValue.Success);
        Assert.Equal(authResult.Token, returnValue.Token);
    }
}
