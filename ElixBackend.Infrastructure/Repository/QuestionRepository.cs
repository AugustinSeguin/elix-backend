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
        return await context.Questions.AsNoTracking()
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<IEnumerable<Question>> GetAllQuestionsAsync()
    {
        return await context.Questions.ToListAsync();
    }

    public async Task<IEnumerable<Question>> GetQuestionsByCategoryIdAsync(int categoryId)
    {
        return await context.Questions
            .Where(q => q.CategoryId == categoryId)
            .Include(q => q.Answers)
            .ToListAsync();
    }

    public async Task<int> GetTotalQuestionByCategoryAsync(int categoryId)
    {
        return await context.Questions.CountAsync(q => q.CategoryId == categoryId);
    }

    public Task<Question> UpdateQuestionAsync(Question question)
    {
        var entry = context.Questions.Update(question);
        return Task.FromResult(entry.Entity);
    }

    public async Task DeleteQuestionAsync(int id)
    {
        var q = await context.Questions.FindAsync(id);
        if (q != null)
            context.Questions.Remove(q);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}