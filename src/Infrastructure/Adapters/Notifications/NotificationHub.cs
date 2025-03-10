
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using MoodTracker_back.Application.Interfaces;

namespace MoodTracker_back.Infrastructure.Adapters.Notifications;

public class NotificationHub : Hub
{
    private readonly IRedisService _redisService;

    public NotificationHub(IRedisService redisService)
    {
        _redisService = redisService;
    }
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await _redisService.StoreConnection(userId, Context.ConnectionId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await _redisService.RemoveConnection(userId, Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}