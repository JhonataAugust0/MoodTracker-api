using Domain.Entities;

namespace Domain.Ports;

public interface IUserRepository
{
    Task<User> GetByIdAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}

public interface IRefreshTokenRepository
{
    Task<RefreshToken> GetByIdAsync(int id);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(int userId);
    Task CreateAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task DeleteAsync(int id);
}

public interface ITagRepository
{
    Task<Tag> GetByIdAsync(int id);
    Task<IEnumerable<Tag>> GetByUserIdAsync(int userId);
    Task CreateAsync(Tag tag);
    Task UpdateAsync(Tag tag);
    Task DeleteAsync(int id);
}

public interface IMoodRepository
{
    Task<Mood> GetByIdAsync(int id);
    Task<IEnumerable<Mood>> GetByUserIdAsync(int userId);
    Task CreateAsync(Mood mood);
    Task UpdateAsync(Mood mood);
    Task DeleteAsync(int id);
}

public interface IHabitRepository
{
    Task<Habit> GetByIdAsync(int id);
    Task<IEnumerable<Habit>> GetByUserIdAsync(int userId);
    Task CreateAsync(Habit habit);
    Task UpdateAsync(Habit habit);
    Task DeleteAsync(int id);
}

public interface IHabitCompletionRepository
{
    Task<HabitCompletion> GetByIdAsync(int id);
    Task<IEnumerable<HabitCompletion>> GetByHabitIdAsync(int habitId);
    Task CreateAsync(HabitCompletion habitCompletion);
    Task UpdateAsync(HabitCompletion habitCompletion);
    Task DeleteAsync(int id);
}

public interface IQuickNoteRepository
{
    Task<QuickNote> GetByIdAsync(int id);
    Task<IEnumerable<QuickNote>> GetByUserIdAsync(int userId);
    Task CreateAsync(QuickNote quickNote);
    Task UpdateAsync(QuickNote quickNote);
    Task DeleteAsync(int id);
}