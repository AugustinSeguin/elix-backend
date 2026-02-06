using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.Extensions.Logging;

namespace ElixBackend.Business.Service;

public class UserPointService(IUserPointRepository userPointRepository, ILogger<UserPointService> logger) : IUserPointService
{
    public async Task<UserPointDto?> GetUserByIdAsync(int id)
    {
        try
        {
            var up = await userPointRepository.GetUserPointByIdAsync(id);
            return up is null ? null : ToDto(up);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UserPointService.GetUserByIdAsync failed for id {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<UserPointDto>?> GetAllUserPointsAsync()
    {
        try
        {
            var list = await userPointRepository.GetAllUserPointsAsync();
            return list.Select(ToDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UserPointService.GetAllUserPointsAsync failed");
            return null;
        }
    }

    public async Task<UserPointDto?> AddUserPointAsync(UserPointDto up)
    {
        try
        {
            var existing = await userPointRepository.GetUserPointsByCategory(up.CategoryId, up.UserId);
            if (existing != null)
            {
                up.Id = existing.Id;
                up.Points += existing.Points;
                return await UpdateUserPointAsync(up);
            }

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
        catch (Exception ex)
        {
            logger.LogError(ex, "UserPointService.AddUserPointAsync failed for {@UserPointDto}", up);
            return null;
        }
    }

    public async Task<UserPointDto?> UpdateUserPointAsync(UserPointDto up)
    {
        try
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
        catch (Exception ex)
        {
            logger.LogError(ex, "UserPointService.UpdateUserPointAsync failed for {@UserPointDto}", up);
            return null;
        }
    }

    public async Task<UserPointDto?> GetUserPointsByCategory(int categoryId, int userId)
    {
        try
        {
            var up = await userPointRepository.GetUserPointsByCategory(categoryId, userId);
            return up is null ? null : ToDto(up);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UserPointService.GetUserPointsByCategory failed for categoryId {CategoryId} and userId {UserId}", categoryId, userId);
            return null;
        }
    }

    public async Task<IEnumerable<UserPointDto>?> GetUserPoints(int userId)
    {
        try
        {
            var list = await userPointRepository.GetUserPoints(userId);
            return list.Select(ToDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UserPointService.GetUserPoints failed for userId {UserId}", userId);
            return null;
        }
    }

    public async Task<int?> GetTotalPointsByUserIdAsync(int userId)
    {
        try
        {
            var list = await userPointRepository.GetUserPoints(userId);
            return list.Sum(up => up.Points);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UserPointService.GetTotalPointsByUserIdAsync failed for userId {UserId}", userId);
            return null;
        }
    }

    public async Task<bool?> DeleteUserPointAsync(int id)
    {
        try
        {
            await userPointRepository.DeleteUserPointAsync(id);
            return await userPointRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UserPointService.DeleteUserPointAsync failed for id {Id}", id);
            return null;
        }
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