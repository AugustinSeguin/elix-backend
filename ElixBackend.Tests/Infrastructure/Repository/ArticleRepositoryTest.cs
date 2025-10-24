using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure;
using ElixBackend.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Tests.Infrastructure.Repository;

[TestFixture]
public class ArticleRepositoryTest
{
    private ElixDbContext _context;
    private ArticleRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ElixDbContext>()
            .UseInMemoryDatabase(databaseName: TestContext.CurrentContext.Test.Name)
            .Options;
        _context = new ElixDbContext(options);
        _repository = new ArticleRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddArticleAsync_AddsArticleAndReturnsIt()
    {
        var article = new Article { Title = "A1", Subtitle = "s", Content = "c", Footer = "f" };

        var result = await _repository.AddArticleAsync(article);
        await _repository.SaveChangesAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("A1"));
        Assert.That(await _context.Articles.AnyAsync(a => a.Title == "A1"), Is.True);
    }

    [Test]
    public async Task GetArticleByIdAsync_ReturnsArticle()
    {
        var article = new Article { Title = "A2", Subtitle = "s" };
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        var result = await _repository.GetArticleByIdAsync(article.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("A2"));
    }

    [Test]
    public async Task GetAllArticlesAsync_ReturnsAllArticles()
    {
        var articles = new List<Article>
        {
            new Article { Title = "X" },
            new Article { Title = "Y" }
        };
        _context.Articles.AddRange(articles);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllArticlesAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task UpdateArticleAsync_UpdatesArticle()
    {
        var article = new Article { Title = "Old", Subtitle = "old" };
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        article.Title = "New";
        await _repository.UpdateArticleAsync(article);
        await _repository.SaveChangesAsync();

        var result = await _repository.GetArticleByIdAsync(article.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("New"));
    }

    [Test]
    public async Task DeleteArticleAsync_RemovesArticle()
    {
        var article = new Article { Id = 1, Title = "ToDelete" };
        await _repository.AddArticleAsync(article);
        await _repository.SaveChangesAsync();

        await _repository.DeleteArticleAsync(1);
        await _repository.SaveChangesAsync();

        var found = await _repository.GetArticleByIdAsync(1);
        Assert.That(found, Is.Null);
    }
}