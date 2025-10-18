namespace ElixBackend.Business.Service;

using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;

public class QuestionService(IQuestionRepository questionRepository) : IQuestionService
{
    public async Task<QuestionDto> AddQuestionAsync(QuestionDto questionDto)
    {
        var question = new Question
        {
            Title = questionDto.Title,
            CategoryId = questionDto.CategoryId,
            MediaPath = questionDto.MediaPath
        };
        var result = await questionRepository.AddQuestionAsync(question);
        await questionRepository.SaveChangesAsync();
        return QuestionDto.QuestionToQuestionDto(result);
    }

    public async Task<QuestionDto?> GetQuestionByIdAsync(int id)
    {
        var q = await questionRepository.GetQuestionByIdAsync(id);
        return q is null ? null : QuestionDto.QuestionToQuestionDto(q);
    }

    public async Task<IEnumerable<QuestionDto>> GetAllQuestionsAsync()
    {
        var questions = await questionRepository.GetAllQuestionsAsync();
        return questions.Select(QuestionDto.QuestionToQuestionDto);
    }

    public async Task<QuestionDto> UpdateQuestionAsync(QuestionDto questionDto)
    {
        var question = new Question
        {
            Id = questionDto.Id,
            Title = questionDto.Title,
            CategoryId = questionDto.CategoryId,
            MediaPath = questionDto.MediaPath
        };
        var result = await questionRepository.UpdateQuestionAsync(question);
        await questionRepository.SaveChangesAsync();
        return QuestionDto.QuestionToQuestionDto(result);
    }

    public async Task DeleteQuestionAsync(int id)
    {
        await questionRepository.DeleteQuestionAsync(id);
        await questionRepository.SaveChangesAsync();
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await questionRepository.SaveChangesAsync();
    }
}