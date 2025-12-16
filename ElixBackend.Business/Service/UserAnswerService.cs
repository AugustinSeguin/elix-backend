using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.Extensions.Logging;

namespace ElixBackend.Business.Service;

public class UserAnswerService(IUserAnswerRepository userAnswerRepository, ILogger<UserAnswerService> logger) : IUserAnswerService
{
    public async Task<IEnumerable<UserAnswerDto>?> GetUserAnswerByUserIdAsync(int userId, int questionId)
    {
        try
        {
            var userAnswers = await userAnswerRepository.GetUserAnswerByUserIdAsync(userId, questionId);

            return userAnswers
                .Where(ua => ua != null)
                .Select(ua => UserAnswerDto.UserAnswerToUserAnswerDto(ua!));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UserAnswerService.GetUserAnswerByUserIdAsync failed for userId {UserId}, questionId {QuestionId}", userId, questionId);
            return null;
        }
    }

    public async Task<UserAnswerDto?> AddUserAsync(UserAnswerDto ue)
    {
        try
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
        catch (Exception ex)
        {
            logger.LogError(ex, "UserAnswerService.AddUserAsync failed for {@UserAnswerDto}", ue);
            return null;
        }
    }

    public async Task<UserAnswerDto?> UpdateUserAsync(UserAnswerDto ue)
    {
        try
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
        catch (Exception ex)
        {
            logger.LogError(ex, "UserAnswerService.UpdateUserAsync failed for {@UserAnswerDto}", ue);
            return null;
        }
    }

    public async Task<bool?> DeleteUserAsync(int id)
    {
        try
        {
            await userAnswerRepository.DeleteUserAnswerAsync(id);
            return await userAnswerRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UserAnswerService.DeleteUserAsync failed for id {Id}", id);
            return null;
        }
    }
}