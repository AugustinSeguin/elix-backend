using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Infrastructure.Repository;

public class UserAnswerRepository(ElixDbContext context) : IUserAnswerRepository
{
    public async Task<UserAnswer> AddUserAnswerAsync(UserAnswer userAnswer)
    {
        var entry = await context.UserAnswers.AddAsync(userAnswer);
        return entry.Entity;
    }

    public async Task<IEnumerable<UserAnswer?>> GetUserAnswerByUserIdAsync(int userId, int questionId)
    {
        return await context.UserAnswers.AsNoTracking().Where(q => q.UserId == userId && q.QuestionId == questionId).ToListAsync();
    }

    public Task<UserAnswer> UpdateUserAnswerAsync(UserAnswer userAnswer)
    {
        var entry = context.UserAnswers.Update(userAnswer);
        return Task.FromResult(entry.Entity);
    }

    public async Task DeleteUserAnswerAsync(int id)
    {
        var ua = await context.UserAnswers.FindAsync(id);
        if (ua != null)
        {
            context.UserAnswers.Remove(ua);
        }
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}