using ElixBackend.Business.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElixBackend.Business.IService;

public interface IQuizService
{
    Task<QuizDto?> GetQuizByIdAsync(int id);
    Task<IEnumerable<QuizDto>> GetAllQuizzesAsync();
    Task<QuizDto> AddQuizAsync(QuizDto quizDto);
    Task<QuizDto> UpdateQuizAsync(QuizDto quizDto);
    Task DeleteQuizAsync(int id);
    Task<bool> SaveChangesAsync();
}