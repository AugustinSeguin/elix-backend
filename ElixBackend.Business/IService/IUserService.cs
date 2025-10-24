using ElixBackend.Business.DTO;
using ElixBackend.Domain.Entities;

namespace ElixBackend.Business.IService;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto?> GetUserByEmailAsync(string? email);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> AddUserAsync(UserDto user);
    Task<UserDto?> UpdateUserAsync(UserDto user);
    Task DeleteUserAsync(int id);
}