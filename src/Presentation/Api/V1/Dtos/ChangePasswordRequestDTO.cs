using System.ComponentModel.DataAnnotations;

namespace MoodTracker_back.Presentation.Api.V1.Dtos;

public class ChangePasswordRequestDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}