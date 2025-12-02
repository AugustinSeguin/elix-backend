using ElixBackend.Business.DTO;

namespace ElixBackend.Business.IService;

public interface IUserPointService
{
    Task<UserPointDto?> GetUserByIdAsync(int id);
    Task<IEnumerable<UserPointDto>> GetAllUsersAsync();
    Task<UserPointDto?> AddUserAsync(UserPointDto up);
    Task<UserPointDto?> UpdateUserAsync(UserPointDto up);
    Task DeleteUserAsync(int id);
}