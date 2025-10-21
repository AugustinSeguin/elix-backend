using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Infrastructure.Repository;

public class QuizRepository(ElixDbContext context) : IQuizRepository
{

    public async Task<Quiz?> GetQuizByIdAsync(int id)
    {
        return await context.Quizzes.Include(q => q.Category).FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<IEnumerable<Quiz>> GetAllQuizzesAsync()
    {
        return await context.Quizzes.Include(q => q.Category).ToListAsync();
    }

    public async Task<Quiz> AddQuizAsync(Quiz quiz)
    {
        var entry = await context.Quizzes.AddAsync(quiz);
        return entry.Entity;
    }

    public Task<Quiz> UpdateQuizAsync(Quiz quiz)
    {
        var entry = context.Quizzes.Update(quiz);
        return Task.FromResult(entry.Entity);
    }

    public async Task DeleteQuizAsync(int id)
    {
        var quiz = await context.Quizzes.FindAsync(id);
        if (quiz != null)
        {
            context.Quizzes.Remove(quiz);
        }
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}