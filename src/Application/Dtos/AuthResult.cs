namespace MoodTracker_back.Application.Dtos;


public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? Error { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; }
}