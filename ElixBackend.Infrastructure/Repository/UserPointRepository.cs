using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Infrastructure.Repository;

public class UserPointRepository(ElixDbContext context) : IUserPointRepository
{
    public async Task<UserPoint> AddUserPointAsync(UserPoint userPoint)
    {
        var entry = await context.UserPoints.AddAsync(userPoint);
        return entry.Entity;
    }

    public async Task<UserPoint?> GetUserPointByIdAsync(int id)
    {
        return await context.UserPoints.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<IEnumerable<UserPoint>> GetAllUserPointsAsync()
    {
        return await context.UserPoints.ToListAsync();
    }

    public Task<UserPoint> UpdateUserPointAsync(UserPoint userPoint)
    {
        var entry = context.UserPoints.Update(userPoint);
        return Task.FromResult(entry.Entity);
    }

    public async Task DeleteUserPointAsync(int id)
    {
        var ua = await context.UserPoints.FindAsync(id);
        if (ua != null)
        {
            context.UserPoints.Remove(ua);
        }
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}