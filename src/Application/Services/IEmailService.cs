namespace MoodTracker_back.Application.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetToken);
}