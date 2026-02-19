using ElixBackend.Business.DTO;

namespace ElixBackend.Business.IService;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto?> GetUserByEmailAsync(string? email, bool includePassword = false);
    Task<IEnumerable<UserDto>?> GetAllUsersAsync();
    Task<UserDto?> AddUserAsync(UserDto user);
    Task<UserDto?> UpdateUserAsync(UserDto user);
    Task<bool?> DeleteUserAsync(int id);
    Task<UserDto?> GetMeAsync(int userId);
}