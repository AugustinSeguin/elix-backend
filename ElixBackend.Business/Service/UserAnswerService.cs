using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;

namespace ElixBackend.Business.Service;

public class UserAnswerService(IUserAnswerRepository userAnswerRepository) : IUserAnswerService
{
    public async Task<IEnumerable<UserAnswerDto?>>GetUserAnswerByUserIdAsync(int userId, int questionId)
    {
        var userAnswers = await userAnswerRepository.GetUserAnswerByUserIdAsync(userId, questionId);
        
        return userAnswers
            .Where(ua => ua != null)
            .Select(ua => UserAnswerDto.UserAnswerToUserAnswerDto(ua!));
    }

    public async Task<UserAnswerDto?> AddUserAsync(UserAnswerDto ue)
    {
        var entity = new UserAnswer
        {
            UserId = ue.UserId,
            QuestionId = ue.QuestionId,
            IsCorrect = ue.IsCorrect
        };

        var added = await userAnswerRepository.AddUserAnswerAsync(entity);
        await userAnswerRepository.SaveChangesAsync();
        return UserAnswerDto.UserAnswerToUserAnswerDto(added);
    }

    public async Task<UserAnswerDto?> UpdateUserAsync(UserAnswerDto ue)
    {
        var entity = new UserAnswer
        {
            Id = ue.Id,
            UserId = ue.UserId,
            QuestionId = ue.QuestionId,
            IsCorrect = ue.IsCorrect
        };

        var updated = await userAnswerRepository.UpdateUserAnswerAsync(entity);
        await userAnswerRepository.SaveChangesAsync();
        return UserAnswerDto.UserAnswerToUserAnswerDto(updated);
    }

    public async Task DeleteUserAsync(int id)
    {
        await userAnswerRepository.DeleteUserAnswerAsync(id);
        await userAnswerRepository.SaveChangesAsync();
    }
}