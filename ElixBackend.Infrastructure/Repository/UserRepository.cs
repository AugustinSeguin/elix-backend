using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Infrastructure.Repository
{
    public class UserRepository(ElixDbContext context) : IUserRepository
    {
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await context.Users.ToListAsync();
        }

        public async Task<User?> AddUserAsync(User user)
        {
            var newUser = await context.Users.AddAsync(user);
            return newUser.Entity;
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            var userUpdated = context.Users.Update(user);
            return userUpdated.Entity;
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user != null)
            {
                context.Users.Remove(user);
            }
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}