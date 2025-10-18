namespace ElixBackend.Business.IService;

using ElixBackend.Business.DTO;

public interface IQuestionService
{
    Task<QuestionDto> AddQuestionAsync(QuestionDto question);

    Task<QuestionDto?> GetQuestionByIdAsync(int id);

    Task<IEnumerable<QuestionDto>> GetAllQuestionsAsync();

    Task<QuestionDto> UpdateQuestionAsync(QuestionDto question);

    Task DeleteQuestionAsync(int id);

    Task<bool> SaveChangesAsync();
}