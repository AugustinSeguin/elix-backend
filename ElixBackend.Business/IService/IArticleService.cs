using ElixBackend.Business.DTO;

namespace ElixBackend.Business.IService;

public interface IArticleService
{
    Task<ArticleDto?> AddArticleAsync(ArticleDto articleDto);

    Task<ArticleDto?> GetArticleByIdAsync(int id);

    Task<IEnumerable<ArticleDto>?> GetAllArticlesAsync();

    Task<IEnumerable<ArticleDto>?> GetArticlesByCategoryAsync(int categoryId);

    Task<ArticleDto?> UpdateArticleAsync(ArticleDto articleDto);

    Task<bool?> DeleteArticleAsync(int id);

    Task<bool?> SaveChangesAsync();
}