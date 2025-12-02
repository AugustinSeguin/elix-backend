using ElixBackend.Business.DTO;

namespace ElixBackend.Business.IService;

public interface IQuizService
{
    Task<QuizDto?> StartQuizAsync(int userId, int categoryId);
    Task<QuizDto?> SubmitQuizAsync(QuizSubmissionDto quizSubmission);
}