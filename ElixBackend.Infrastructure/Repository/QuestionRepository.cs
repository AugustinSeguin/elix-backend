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
        return await context.Questions.FindAsync(id);
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

    public Task DeleteQuestionAsync(int id)
    {
        context.Questions.Remove(new Question { Id = id });
        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}