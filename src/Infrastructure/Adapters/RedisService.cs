using MoodTracker_back.Application.Services;
using StackExchange.Redis;

namespace MoodTracker_back.Infrastructure.Adapters;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task StoreConnection(string userId, string connectionId)
    {
        var db = _redis.GetDatabase();
        await db.SetAddAsync($"user:{userId}:connections", connectionId);
    }

    public async Task RemoveConnection(string userId, string connectionId)
    {
        var db = _redis.GetDatabase();
        await db.SetRemoveAsync($"user:{userId}:connections", connectionId);
    }

    public async Task<IEnumerable<string>> GetConnectionIds(int userId)
    {
        var db = _redis.GetDatabase();
        var result = await db.SetMembersAsync($"user:{userId}:connections");
        return result.Select(r => r.ToString());
    }

    public async Task StoreNotification(int userId, string message)
    {
        var db = _redis.GetDatabase();
        await db.ListRightPushAsync($"user:{userId}:notifications", message);
    }

    public async Task<IEnumerable<string>> GetNotifications(int userId)
    {
        var db = _redis.GetDatabase();
        var result = await db.ListRangeAsync($"user:{userId}:notifications");
        return result.Select(r => r.ToString());
    }
    
    public async Task RemoveNotifications(int userId)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync($"user:{userId}:notifications");
    }

    public async Task<bool> RemoveNotification(int userId, string notificationId)
    {
        var db = _redis.GetDatabase();
        return await db.ListRemoveAsync($"user:{userId}:notifications", notificationId) > 0;
    }
}