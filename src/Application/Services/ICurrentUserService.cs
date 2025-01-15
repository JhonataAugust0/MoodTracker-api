namespace MoodTracker_back.Application.Services;


public interface ICurrentUserService
{
    int UserId { get; }
    string? UserEmail { get; }
    bool IsAuthenticated { get; }
}