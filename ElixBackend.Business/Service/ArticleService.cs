using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Infrastructure.IRepository;

namespace ElixBackend.Business.Service;

public class ArticleService(IArticleRepository articleRepository) : IArticleService
{
    public async Task<ArticleDto> AddArticleAsync(ArticleDto articleDto)
    {
        var article = articleDto.ToEntity();
        var result = await articleRepository.AddArticleAsync(article);
        await articleRepository.SaveChangesAsync();
        return ArticleDto.FromEntity(result);
    }

    public async Task<ArticleDto?> GetArticleByIdAsync(int id)
    {
        var a = await articleRepository.GetArticleByIdAsync(id);
        return a is null ? null : ArticleDto.FromEntity(a);
    }

    public async Task<IEnumerable<ArticleDto>> GetAllArticlesAsync()
    {
        var articles = await articleRepository.GetAllArticlesAsync();
        return articles.Select(ArticleDto.FromEntity);
    }

    public async Task<ArticleDto> UpdateArticleAsync(ArticleDto articleDto)
    {
        var article = articleDto.ToEntity();
        var result = await articleRepository.UpdateArticleAsync(article);
        await articleRepository.SaveChangesAsync();
        return ArticleDto.FromEntity(result);
    }

    public async Task DeleteArticleAsync(int id)
    {
        await articleRepository.DeleteArticleAsync(id);
        await articleRepository.SaveChangesAsync();
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await articleRepository.SaveChangesAsync();
    }
}