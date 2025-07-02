using ElixBackend.Domain.Entities;

namespace ElixBackend.Infrastructure.IRepository
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> AddUserAsync(User user);
        Task<User?> UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task<bool> UserExistsAsync(int id);
        Task<bool> SaveChangesAsync();
    }
}