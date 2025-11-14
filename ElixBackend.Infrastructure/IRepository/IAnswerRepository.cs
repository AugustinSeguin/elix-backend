using ElixBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Infrastructure.IRepository;

public interface IAnswerRepository
{
    Task<Answer?> GetByIdAsync(int id);
    Task<IEnumerable<Answer>> GetAllAsync();
    Task<IEnumerable<Answer>> GetByQuestionIdAsync(int questionId);
    Task AddAsync(Answer answer);
    Task UpdateAsync(Answer answer);
    Task DeleteAsync(int id);
}