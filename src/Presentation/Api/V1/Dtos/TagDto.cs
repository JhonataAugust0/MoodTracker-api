using System.ComponentModel.DataAnnotations;

namespace MoodTracker_back.Presentation.Api.V1.Dtos;

public class TagDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#FFFFFF";
    public DateTimeOffset CreatedAt { get; set; }
}

public class CreateTagDto
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#FFFFFF";
    public DateTimeOffset? Timestamp { get; set; }
}

public class UpdateTagDto
{
    public string? Name { get; set; }
    public string? Color { get; set; }
}