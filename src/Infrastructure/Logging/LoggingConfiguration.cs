namespace MoodTracker_back.Infrastructure.Logging;

public class LoggingConfiguration
{
public static void ConfigureLogging(WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        
        builder.Logging.AddConsole();
        
        builder.Logging.AddFile(
            pathFormat: "Logs/api-{Date}.txt", 
            minimumLevel: LogLevel.Information, 
            levelOverrides: null, 
            isJson: true, 
            fileSizeLimitBytes: 5 * 1024 * 1024,
            retainedFileCountLimit: 30, 
            outputTemplate: "{Timestamp:o} {RequestId,13} [{Level:u3}] {Message} ({EventId:x8}){NewLine}{Exception}"
        );

        builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
        builder.Logging.AddFilter("System", LogLevel.Warning);
        builder.Logging.AddFilter("Application", LogLevel.Information);
    }
}
