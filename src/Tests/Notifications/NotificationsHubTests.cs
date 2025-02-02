using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Presentation.Api.V1.Controllers;

namespace MoodTracker_back.Tests.Notifications;

public class NotificationsHubTests
{
    private readonly Mock<IRedisService> _redisServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly NotificationsController _controller;

    public NotificationsHubTests()
    {
        _redisServiceMock = new Mock<IRedisService>();
        _userServiceMock = new Mock<IUserService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _controller = new NotificationsController(
            _redisServiceMock.Object,
            _userServiceMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task GetNotifications_ShouldReturnOk_WhenNotificationsExist()
    {
        var fakeNotifications = new List<string> { "Notificação 1", "Notificação 2" };
        _currentUserServiceMock.Setup(u => u.UserId).Returns(1);
        _redisServiceMock.Setup(r => r.GetNotifications(1)).ReturnsAsync(fakeNotifications);

        var result = await _controller.GetNotifications();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedNotifications = Assert.IsType<List<string>>(okResult.Value);
        Assert.Equal(2, returnedNotifications.Count);
    }

    [Fact]
    public async Task MarkNotificationsAsRead_ShouldReturnNoContent()
    {
        _currentUserServiceMock.Setup(u => u.UserId).Returns(1);
        _redisServiceMock.Setup(r => r.RemoveNotifications(1)).Returns(Task.CompletedTask);

        var result = await _controller.MarkNotificationsAsRead();

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteNotification_ShouldReturnNoContent_WhenNotificationIsRemoved()
    {
        _currentUserServiceMock.Setup(u => u.UserId).Returns(1);
        _redisServiceMock.Setup(r => r.RemoveNotification(1, "123")).ReturnsAsync(true);

        var result = await _controller.DeleteNotification("123");

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteNotification_ShouldReturnNotFound_WhenNotificationDoesNotExist()
    {
        _currentUserServiceMock.Setup(u => u.UserId).Returns(1);
        _redisServiceMock.Setup(r => r.RemoveNotification(1, "123")).ReturnsAsync(false);

        var result = await _controller.DeleteNotification("123");

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Notificação não encontrada", notFoundResult.Value.GetType().GetProperty("message")?.GetValue(notFoundResult.Value));
    }
}
