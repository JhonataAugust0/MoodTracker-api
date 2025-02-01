namespace MoodTracker_back.Application.Interfaces;


public interface ILoggingService
{
    Task LogInformationAsync(string message, params object[] args);
    Task LogWarningAsync(string message, params object[] args);
    Task LogErrorAsync(Exception exception, string message, params object[] args);
    Task LogDebugAsync(string message, params object[] args);
}