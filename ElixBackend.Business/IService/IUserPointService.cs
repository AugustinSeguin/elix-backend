using ElixBackend.Business.DTO;

namespace ElixBackend.Business.IService;

public interface IUserPointService
{
    Task<UserPointDto?> GetUserByIdAsync(int id);
    Task<IEnumerable<UserPointDto>> GetAllUserPointsAsync();
    Task<UserPointDto?> AddUserPointAsync(UserPointDto up);
    Task<UserPointDto?> UpdateUserPointAsync(UserPointDto up);
    Task DeleteUserPointAsync(int id);
}