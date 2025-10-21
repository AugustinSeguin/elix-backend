using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Infrastructure.Repository;

public class AnswerRepository(ElixDbContext context) : IAnswerRepository
{
    public async Task<Answer?> GetByIdAsync(int id)
    {
        return await context.Set<Answer>().FindAsync(id);
    }

    public async Task<IEnumerable<Answer>> GetAllAsync()
    {
        return await context.Set<Answer>().ToListAsync();
    }

    public async Task AddAsync(Answer answer)
    {
        await context.Set<Answer>().AddAsync(answer);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Answer answer)
    {
        context.Set<Answer>().Update(answer);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var answer = await GetByIdAsync(id);
        if (answer != null)
        {
            context.Set<Answer>().Remove(answer);
            await context.SaveChangesAsync();
        }
    }
}