namespace MoodTracker_back.Application.Dtos;


public class UserResponseDto
{
    public bool Success { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
}