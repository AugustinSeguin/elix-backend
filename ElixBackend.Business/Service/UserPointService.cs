using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;

namespace ElixBackend.Business.Service;

public class UserPointService(IUserPointRepository userPointRepository) : IUserPointService
{
    public async Task<UserPointDto?> GetUserByIdAsync(int id)
    {
        var up = await userPointRepository.GetUserPointByIdAsync(id);
        return up is null ? null : ToDto(up);
    }

    public async Task<IEnumerable<UserPointDto>> GetAllUsersAsync()
    {
        var list = await userPointRepository.GetAllUserPointsAsync();
        return list.Select(ToDto);
    }

    public async Task<UserPointDto?> AddUserAsync(UserPointDto up)
    {
        var entity = new UserPoint
        {
            UserId = up.UserId,
            CategoryId = up.CategoryId,
            Points = up.Points
        };

        var added = await userPointRepository.AddUserPointAsync(entity);
        await userPointRepository.SaveChangesAsync();
        return ToDto(added);
    }

    public async Task<UserPointDto?> UpdateUserAsync(UserPointDto up)
    {
        var entity = new UserPoint
        {
            Id = up.Id,
            UserId = up.UserId,
            CategoryId = up.CategoryId,
            Points = up.Points
        };

        var updated = await userPointRepository.UpdateUserPointAsync(entity);
        await userPointRepository.SaveChangesAsync();
        return ToDto(updated);
    }

    public async Task DeleteUserAsync(int id)
    {
        await userPointRepository.DeleteUserPointAsync(id);
        await userPointRepository.SaveChangesAsync();
    }

    private static UserPointDto ToDto(UserPoint up)
    {
        return new UserPointDto
        {
            Id = up.Id,
            UserId = up.UserId,
            CategoryId = up.CategoryId,
            Points = up.Points
        };
    }
}