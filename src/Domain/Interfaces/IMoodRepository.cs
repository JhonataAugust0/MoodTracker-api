using Domain.Entities;

namespace Domain.Interfaces;


public interface IMoodRepository
{
    Task<Mood> GetByIdAsync(int id);
    Task<IEnumerable<Mood>> GetByUserIdAsync(int userId);
    Task CreateAsync(Mood mood);
    Task UpdateAsync(Mood mood);
    Task DeleteAsync(int id);
}