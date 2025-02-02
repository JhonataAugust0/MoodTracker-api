using System.ComponentModel.DataAnnotations;
using MoodTracker_back.Domain.Entities;

namespace MoodTracker_back.Presentation.Api.V1.Dtos;

public class HabitDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public ICollection<TagDto> Tags { get; set; } = new List<TagDto>();
    public int FrequencyTarget { get; set; } = 1;
    public FrequencyType FrequencyType { get; set; } = FrequencyType.Daily;
    public string Color { get; set; }
}

public class CreateHabitDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public ICollection<int>? TagIds { get; set; } = new List<int>();
    public DateTimeOffset? CreatedAt { get; set; }
    public int FrequencyTarget { get; set; } = 1;
    public FrequencyType FrequencyType { get; set; } = FrequencyType.Daily;
    public string Color { get; set; }
}

public class UpdateHabitDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public ICollection<int>? TagIds { get; set; }
    public int FrequencyTarget { get; set; } = 1;
    public FrequencyType FrequencyType { get; set; } = FrequencyType.Daily;
    public string Color { get; set; }
}

public class LogHabitCompletionDto
{
    public int HabitId { get; set; }
    public DateTimeOffset CompletedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? Notes { get; set; }
}

public class HabitCompletionDto
{
    public int Id { get; set; }
    public int HabitId { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public string? Notes { get; set; }
}
