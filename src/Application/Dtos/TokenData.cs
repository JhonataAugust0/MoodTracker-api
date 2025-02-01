namespace MoodTracker_back.Application.Dtos;

public class TokenData
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public long ExpiresAt { get; set; }
}