using ElixBackend.Business.DTO;

namespace ElixBackend.Business.IService;

public interface IUserAnswerService
{
    Task<UserAnswerDto?> GetUserByIdAsync(int id);
    Task<IEnumerable<UserAnswerDto>> GetAllUsersAsync();
    Task<UserAnswerDto?> AddUserAsync(UserAnswerDto ue);
    Task<UserAnswerDto?> UpdateUserAsync(UserAnswerDto ue);
    Task DeleteUserAsync(int id);
}