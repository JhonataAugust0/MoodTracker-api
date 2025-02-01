using System.ComponentModel.DataAnnotations;

namespace MoodTracker_back.Presentation.Api.V1.Dtos;

public class QuickNoteDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public List<TagDto> Tags { get; set; } = new List<TagDto>();
}

public class CreateQuickNoteDto
{
    public string Content { get; set; } = string.Empty;
    public List<int> TagIds { get; set; } = new List<int>();
}

public class UpdateQuickNoteDto
{
    public string Content { get; set; } = string.Empty;
    public List<int> TagIds { get; set; } = new List<int>();
    public bool IsDeleted { get; set; } = false;
}
