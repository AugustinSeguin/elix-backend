using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.Extensions.Logging;

namespace ElixBackend.Business.Service;

public class AnswerService(IAnswerRepository answerRepository, ILogger<AnswerService> _logger) : IAnswerService
{
    public async Task<AnswerDto?> GetByIdAsync(int id)
    {
        try
        {
            var answer = await answerRepository.GetByIdAsync(id);
            return answer == null ? null : ToDto(answer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AnswerService.GetByIdAsync failed for id {Id}", id);
        }

        return null;
    }

    public async Task<IEnumerable<AnswerDto>?> GetAllAsync()
    {
        try
        {
            var answers = await answerRepository.GetAllAsync();
            return answers.Select(ToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AnswerService.GetAllAsync failed");
        }
        return null;
    }
    
    public async Task<IEnumerable<AnswerDto>?> GetByQuestionIdAsync(int questionId)
    {
        try
        {
            var answers = await answerRepository.GetByQuestionIdAsync(questionId);
            return answers.Select(ToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AnswerService.GetByQuestionIdAsync failed for questionId {QuestionId}", questionId);
        }

        return null;
    }

    public async Task AddAsync(AnswerDto answerDto)
    {
        try
        {
            var answer = FromDto(answerDto);
            await answerRepository.AddAsync(answer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AnswerService.AddAsync failed for AnswerDto {@AnswerDto}", answerDto);
        }
    }

    public async Task UpdateAsync(AnswerDto answerDto)
    {
        try
        {
            var answer = FromDto(answerDto);
            await answerRepository.UpdateAsync(answer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AnswerService.UpdateAsync failed for AnswerDto {@AnswerDto}", answerDto);
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            await answerRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AnswerService.DeleteAsync failed for id {Id}", id);
        }
    }

    private static AnswerDto ToDto(Answer answer)
    {
        return new AnswerDto
        {
            Id = answer.Id,
            QuestionId = answer.QuestionId,
            Title = answer.Title,
            IsValid = answer.IsValid,
            Explanation = answer.Explanation
        };
    }

    private static Answer FromDto(AnswerDto dto)
    {
        return new Answer
        {
            Id = dto.Id,
            QuestionId = dto.QuestionId,
            Title = dto.Title,
            IsValid = dto.IsValid,
            Explanation = dto.Explanation
        };
    }
}