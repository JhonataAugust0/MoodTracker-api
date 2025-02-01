namespace MoodTracker_back.Application.Services;

public interface IRedisService
{
    Task StoreConnection(string userId, string connectionId);
    Task RemoveConnection(string userId, string connectionId);
    Task<IEnumerable<string>> GetConnectionIds(int userId);
    Task StoreNotification(int userId, string message);
    Task<IEnumerable<string>> GetNotifications(int userId);
    Task RemoveNotifications(int userId);
    Task<bool> RemoveNotification(int userId, string notificationId);
}