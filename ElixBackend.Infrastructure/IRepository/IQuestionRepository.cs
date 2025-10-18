namespace ElixBackend.Infrastructure.IRepository;

using System.Collections.Generic;
using System.Threading.Tasks;
using ElixBackend.Domain.Entities;

public interface IQuestionRepository
{
    Task<Question> AddQuestionAsync(Question question);

    Task<Question?> GetQuestionByIdAsync(int id);

    Task<IEnumerable<Question>> GetAllQuestionsAsync();

    Task<Question> UpdateQuestionAsync(Question question);

    Task DeleteQuestionAsync(int id);

    Task<bool> SaveChangesAsync();
}