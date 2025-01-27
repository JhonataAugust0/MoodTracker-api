namespace Domain.Entities
{
    public enum FrequencyType
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Yearly = 4,
        Custom = 5
    }

    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string Preferences { get; set; } = "{}";
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }

    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public User User { get; set; } = null!;
    }

    public class Tag
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#FFFFFF";
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public User User { get; set; } = null!;
    }

    public class Mood
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string MoodType { get; set; } = string.Empty;
        public int Intensity { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }

    public class Habit
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsActive { get; set; } = true;
        public int FrequencyTarget { get; set; } = 1;
        public string Color { get; set; }
        public FrequencyType FrequencyType { get; set; } = FrequencyType.Daily;
        public User User { get; set; } = null!;
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }

    public class HabitCompletion
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public DateTimeOffset CompletedAt { get; set; } = DateTimeOffset.UtcNow;
        public string? Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public Habit Habit { get; set; } = null!;
    }

    public class QuickNote
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public bool IsDeleted { get; set; } = false;

        public User User { get; set; } = null!;
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}