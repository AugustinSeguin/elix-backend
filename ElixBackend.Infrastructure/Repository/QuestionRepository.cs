using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Infrastructure.Repository;

public class QuestionRepository(ElixDbContext context) : IQuestionRepository
{
    public async Task<Question> AddQuestionAsync(Question question)
    {
        var entry = await context.Questions.AddAsync(question);
        return entry.Entity;
    }

    public async Task<Question?> GetQuestionByIdAsync(int id)
    {
        return await context.Questions.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<IEnumerable<Question>> GetAllQuestionsAsync()
    {
        return await context.Questions.ToListAsync();
    }

    public Task<Question> UpdateQuestionAsync(Question question)
    {
        var entry = context.Questions.Update(question);
        return Task.FromResult(entry.Entity);
    }

    public async Task DeleteQuestionAsync(int id)
    {
        var question = await context.Questions.FindAsync(id);
        if (question != null)
        {
            context.Questions.Remove(question);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}