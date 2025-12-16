using ElixBackend.Business.DTO;

namespace ElixBackend.Business.IService;

public interface IUserAnswerService
{
    Task<IEnumerable<UserAnswerDto>?> GetUserAnswerByUserIdAsync(int userId, int questionId);
    Task<UserAnswerDto?> AddUserAsync(UserAnswerDto ue);
    Task<UserAnswerDto?> UpdateUserAsync(UserAnswerDto ue);
    Task<bool?> DeleteUserAsync(int id);
}