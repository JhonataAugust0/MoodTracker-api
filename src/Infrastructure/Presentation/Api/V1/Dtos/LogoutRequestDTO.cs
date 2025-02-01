using System.ComponentModel.DataAnnotations;

namespace MoodTracker_back.Presentation.Api.V1.Dtos;

public class LogoutRequestDTO
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}