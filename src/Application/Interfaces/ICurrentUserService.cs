namespace MoodTracker_back.Application.Interfaces;


public interface ICurrentUserService
{
    int UserId { get; }
    string? UserEmail { get; }
    bool IsAuthenticated { get; }
}