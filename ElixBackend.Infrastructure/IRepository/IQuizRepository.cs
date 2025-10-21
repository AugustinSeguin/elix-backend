using ElixBackend.Domain.Entities;

namespace ElixBackend.Infrastructure.IRepository;

public interface IQuizRepository
{
    Task<Quiz?> GetQuizByIdAsync(int id);
    Task<IEnumerable<Quiz>> GetAllQuizzesAsync();
    Task<Quiz> AddQuizAsync(Quiz quiz);
    Task<Quiz> UpdateQuizAsync(Quiz quiz);
    Task DeleteQuizAsync(int id);
    Task<bool> SaveChangesAsync();
}