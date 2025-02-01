using MoodTracker_back.Application.Interfaces;

namespace MoodTracker_back.Infrastructure.Logging;

public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;

    public LoggingService(ILogger<LoggingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task LogInformationAsync(string message, params object[] args)
    {
        await Task.Run(() =>
        {
            _logger.LogInformation(message, args);
        });
    }

    public async Task LogWarningAsync(string message, params object[] args)
    {
        await Task.Run(() =>
        {
            _logger.LogWarning(message, args);
        });
    }

    public async Task LogErrorAsync(Exception exception, string message, params object[] args)
    {
        await Task.Run(() =>
        {
            _logger.LogError(exception, message, args);
        });
    }

    public async Task LogDebugAsync(string message, params object[] args)
    {
        await Task.Run(() =>
        {
            _logger.LogDebug(message, args);
        });
    }
}