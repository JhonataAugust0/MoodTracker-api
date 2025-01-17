namespace MoodTracker_back.Application.Services;

public interface IPasswordService
{
    string HashPassword(string password, out string salt);
    bool VerifyPassword(string password, string hash, string salt);
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword);
}