namespace MoodTracker_back.Application.Services;

public interface IEmailService
{
    Task<bool> SendPasswordRecoverEmailAsync(string mailTo, string link);
    Task<bool> SendAccounAccessedEmailAsync(string mailTo);
    Task<bool> SendNotificationEmail(string mailTo);
}