namespace ElixBackend.Infrastructure.IRepository;

using System.Collections.Generic;
using System.Threading.Tasks;
using ElixBackend.Domain.Entities;

public interface IArticleRepository
{
    Task<Article> AddArticleAsync(Article article);

    Task<Article?> GetArticleByIdAsync(int id);

    Task<IEnumerable<Article>> GetAllArticlesAsync();

    Task<IEnumerable<Article>> GetArticlesByCategoryAsync(int categoryId);

    Task<Article> UpdateArticleAsync(Article article);

    Task DeleteArticleAsync(int id);

    Task<bool> SaveChangesAsync();
}