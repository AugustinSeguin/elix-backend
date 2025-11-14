using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;

namespace ElixBackend.Business.Service;

public class AnswerService(IAnswerRepository answerRepository) : IAnswerService
{
    public async Task<AnswerDto?> GetByIdAsync(int id)
    {
        var answer = await answerRepository.GetByIdAsync(id);
        return answer == null ? null : ToDto(answer);
    }

    public async Task<IEnumerable<AnswerDto>> GetAllAsync()
    {
        var answers = await answerRepository.GetAllAsync();
        return answers.Select(ToDto);
    }
    
    public async Task<IEnumerable<AnswerDto>> GetByQuestionIdAsync(int questionId)
    {
        var answers = await answerRepository.GetByQuestionIdAsync(questionId);
        return answers.Select(ToDto);
    }

    public async Task AddAsync(AnswerDto answerDto)
    {
        var answer = FromDto(answerDto);
        await answerRepository.AddAsync(answer);
    }

    public async Task UpdateAsync(AnswerDto answerDto)
    {
        var answer = FromDto(answerDto);
        await answerRepository.UpdateAsync(answer);
    }

    public async Task DeleteAsync(int id)
    {
        await answerRepository.DeleteAsync(id);
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