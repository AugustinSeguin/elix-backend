using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;

namespace ElixBackend.Business.Service;

public class UserAnswerService(IUserAnswerRepository userAnswerRepository) : IUserAnswerService
{
    public async Task<UserAnswerDto?> GetUserByIdAsync(int id)
    {
        var ua = await userAnswerRepository.GetUserAnswerByIdAsync(id);
        return ua is null ? null : UserAnswerDto.QuestionToQuestionDto(ua);
    }

    public async Task<IEnumerable<UserAnswerDto>> GetAllUsersAsync()
    {
        var list = await userAnswerRepository.GetAllUserAnswersAsync();
        return list.Select(UserAnswerDto.QuestionToQuestionDto);
    }

    public async Task<UserAnswerDto?> AddUserAsync(UserAnswerDto ue)
    {
        var entity = new UserAnswer
        {
            UserId = ue.UserId,
            AnswerId = ue.AnswerId,
            IsCorrect = ue.IsCorrect
        };

        var added = await userAnswerRepository.AddUserAnswerAsync(entity);
        await userAnswerRepository.SaveChangesAsync();
        return UserAnswerDto.QuestionToQuestionDto(added);
    }

    public async Task<UserAnswerDto?> UpdateUserAsync(UserAnswerDto ue)
    {
        var entity = new UserAnswer
        {
            Id = ue.Id,
            UserId = ue.UserId,
            AnswerId = ue.AnswerId,
            IsCorrect = ue.IsCorrect
        };

        var updated = await userAnswerRepository.UpdateUserAnswerAsync(entity);
        await userAnswerRepository.SaveChangesAsync();
        return UserAnswerDto.QuestionToQuestionDto(updated);
    }

    public async Task DeleteUserAsync(int id)
    {
        await userAnswerRepository.DeleteUserAnswerAsync(id);
        await userAnswerRepository.SaveChangesAsync();
    }
}