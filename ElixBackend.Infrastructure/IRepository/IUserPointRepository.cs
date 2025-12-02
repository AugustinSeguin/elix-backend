using ElixBackend.Domain.Entities;

namespace ElixBackend.Infrastructure.IRepository;

public interface IUserPointRepository
{
    Task<UserPoint> AddUserPointAsync(UserPoint userPoint);

    Task<UserPoint?> GetUserPointByIdAsync(int id);

    Task<IEnumerable<UserPoint>> GetAllUserPointsAsync();

    Task<UserPoint> UpdateUserPointAsync(UserPoint userPoint);

    Task DeleteUserPointAsync(int id);

    Task<bool> SaveChangesAsync();
}