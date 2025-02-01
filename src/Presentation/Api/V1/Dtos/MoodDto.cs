using System.ComponentModel.DataAnnotations;

namespace MoodTracker_back.Presentation.Api.V1.Dtos;

public class MoodDto
{
    public int Id { get; set; }
    public string MoodType { get; set; } = string.Empty;
    public int Intensity { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public List<int> TagIds { get; set; } = new List<int>();
}

public class CreateMoodDto
{
    public string MoodType { get; set; } = string.Empty;
    public int Intensity { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public List<int> TagIds { get; set; } = new List<int>();
}

public class UpdateMoodDto
{
    public string? MoodType { get; set; }
    public int? Intensity { get; set; }
    public string? Notes { get; set; }
    public List<int>? TagIds { get; set; }
}