using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.Extensions.Logging;

namespace ElixBackend.Business.Service;

public class ArticleService(IArticleRepository articleRepository, ILogger<ArticleService> logger) : IArticleService
{
    public async Task<ArticleDto?> AddArticleAsync(ArticleDto articleDto)
    {
        try
        {
            var article = articleDto.ToEntity();
            var result = await articleRepository.AddArticleAsync(article);
            await articleRepository.SaveChangesAsync();
            return ArticleDto.FromEntity(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ArticleService.AddArticleAsync failed for {@ArticleDto}", articleDto);
            return null;
        }
    }

    public async Task<ArticleDto?> GetArticleByIdAsync(int id)
    {
        try
        {
            var a = await articleRepository.GetArticleByIdAsync(id);
            return a is null ? null : ArticleDto.FromEntity(a);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ArticleService.GetArticleByIdAsync failed for id {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<ArticleDto>?> GetAllArticlesAsync()
    {
        try
        {
            var articles = await articleRepository.GetAllArticlesAsync();
            return articles.Select(ArticleDto.FromEntity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ArticleService.GetAllArticlesAsync failed");
            return null;
        }
    }

    public async Task<ArticleDto?> UpdateArticleAsync(ArticleDto articleDto)
    {
        try
        {
            var article = articleDto.ToEntity();
            var result = await articleRepository.UpdateArticleAsync(article);
            await articleRepository.SaveChangesAsync();
            return ArticleDto.FromEntity(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ArticleService.UpdateArticleAsync failed for {@ArticleDto}", articleDto);
            return null;
        }
    }

    public async Task<bool?> DeleteArticleAsync(int id)
    {
        try
        {
            await articleRepository.DeleteArticleAsync(id);
            var saved = await articleRepository.SaveChangesAsync();
            return saved;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ArticleService.DeleteArticleAsync failed for id {Id}", id);
            return null;
        }
    }

    public async Task<bool?> SaveChangesAsync()
    {
        try
        {
            return await articleRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ArticleService.SaveChangesAsync failed");
            return null;
        }
    }
}