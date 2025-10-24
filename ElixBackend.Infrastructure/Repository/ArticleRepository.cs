using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Infrastructure.Repository;

public class ArticleRepository(ElixDbContext context) : IArticleRepository
{
    public async Task<Article> AddArticleAsync(Article article)
    {
        var entry = await context.Articles.AddAsync(article);
        return entry.Entity;
    }

    public async Task<Article?> GetArticleByIdAsync(int id)
    {
        return await context.Articles.FindAsync(id);
    }

    public async Task<IEnumerable<Article>> GetAllArticlesAsync()
    {
        return await context.Articles.ToListAsync();
    }

    public Task<Article> UpdateArticleAsync(Article article)
    {
        var entry = context.Articles.Update(article);
        return Task.FromResult(entry.Entity);
    }

    public async Task DeleteArticleAsync(int id)
    {
        var article = await context.Articles.FindAsync(id);
        if (article != null)
        {
            context.Articles.Remove(article);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}