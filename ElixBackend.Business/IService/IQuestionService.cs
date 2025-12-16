namespace ElixBackend.Business.IService;

using DTO;

public interface IQuestionService
{
    Task<QuestionDto?> AddQuestionAsync(QuestionDto question);

    Task<QuestionDto?> GetQuestionByIdAsync(int id);

    Task<IEnumerable<QuestionDto>?> GetAllQuestionsAsync();
    
    Task<IEnumerable<QuestionDto>?> GetQuestionsByCategoryIdAsync(int categoryId);

    Task<QuestionDto?> UpdateQuestionAsync(QuestionDto question);

    Task<bool?> DeleteQuestionAsync(int id);

    Task<bool?> SaveChangesAsync();
}