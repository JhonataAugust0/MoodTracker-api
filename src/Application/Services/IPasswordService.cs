namespace MoodTracker_back.Application.Services;

public interface IPasswordService
{
    string HashPassword(string password, out string salt);
    bool VerifyPassword(string password, string hash, string salt);
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ChangePasswordAsync(string email, string token, string password, string newPassword);
}