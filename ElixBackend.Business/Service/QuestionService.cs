namespace ElixBackend.Business.Service;

using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.Extensions.Logging;

public class QuestionService(IQuestionRepository questionRepository, ILogger<QuestionService> logger) : IQuestionService
{
    public async Task<QuestionDto?> AddQuestionAsync(QuestionDto questionDto)
    {
        try
        {
            var question = new Question
            {
                Title = questionDto.Title,
                MediaPath = questionDto.MediaPath,
                TypeQuestion = questionDto.TypeQuestion,
                CategoryId = questionDto.CategoryId
            };
            var result = await questionRepository.AddQuestionAsync(question);
            await questionRepository.SaveChangesAsync();
            return QuestionDto.QuestionToQuestionDto(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QuestionService.AddQuestionAsync failed for {@QuestionDto}", questionDto);
            return null;
        }
    }

    public async Task<QuestionDto?> GetQuestionByIdAsync(int id)
    {
        try
        {
            var q = await questionRepository.GetQuestionByIdAsync(id);
            return q is null ? null : QuestionDto.QuestionToQuestionDto(q);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QuestionService.GetQuestionByIdAsync failed for id {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<QuestionDto>?> GetAllQuestionsAsync()
    {
        try
        {
            var questions = await questionRepository.GetAllQuestionsAsync();
            return questions.Select(QuestionDto.QuestionToQuestionDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QuestionService.GetAllQuestionsAsync failed");
            return null;
        }
    }

    public async Task<IEnumerable<QuestionDto>?> GetQuestionsByCategoryIdAsync(int categoryId)
    {
        try
        {
            var questions = await questionRepository.GetQuestionsByCategoryIdAsync(categoryId);
            return questions.Select(QuestionDto.QuestionToQuestionDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QuestionService.GetQuestionsByCategoryIdAsync failed for categoryId {CategoryId}", categoryId);
            return null;
        }
    }

    public async Task<QuestionDto?> UpdateQuestionAsync(QuestionDto questionDto)
    {
        try
        {
            var question = new Question
            {
                Id = questionDto.Id,
                Title = questionDto.Title,
                MediaPath = questionDto.MediaPath,
                TypeQuestion = questionDto.TypeQuestion,
                CategoryId = questionDto.CategoryId
            };
            var result = await questionRepository.UpdateQuestionAsync(question);
            await questionRepository.SaveChangesAsync();
            return QuestionDto.QuestionToQuestionDto(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QuestionService.UpdateQuestionAsync failed for {@QuestionDto}", questionDto);
            return null;
        }
    }

    public async Task<bool?> DeleteQuestionAsync(int id)
    {
        try
        {
            await questionRepository.DeleteQuestionAsync(id);
            var saved = await questionRepository.SaveChangesAsync();
            return saved;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QuestionService.DeleteQuestionAsync failed for id {Id}", id);
            return null;
        }
    }

    public async Task<bool?> SaveChangesAsync()
    {
        try
        {
            return await questionRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QuestionService.SaveChangesAsync failed");
            return null;
        }
    }
}