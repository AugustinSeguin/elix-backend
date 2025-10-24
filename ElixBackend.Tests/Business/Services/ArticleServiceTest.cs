using ElixBackend.Business.DTO;
using ElixBackend.Business.Service;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Moq;

namespace ElixBackend.Tests.Business.Services;

[TestFixture]
public class ArticleServiceTest
{
    private Mock<IArticleRepository> _articleRepositoryMock;
    private ArticleService _articleService;

    [SetUp]
    public void SetUp()
    {
        _articleRepositoryMock = new Mock<IArticleRepository>();
        _articleService = new ArticleService(_articleRepositoryMock.Object);
    }

    [Test]
    public async Task AddArticleAsync_CallsRepositoryAndReturnsDto()
    {
        var dto = new ArticleDto { Title = "A" };
        var article = new Article { Id = 1, Title = "A" };
        _articleRepositoryMock.Setup(r => r.AddArticleAsync(It.IsAny<Article>())).ReturnsAsync(article);
        _articleRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _articleService.AddArticleAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(article.Id));
        Assert.That(result.Title, Is.EqualTo(article.Title));
        _articleRepositoryMock.Verify(r => r.AddArticleAsync(It.Is<Article>(a => a.Title == dto.Title)), Times.Once);
        _articleRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetArticleByIdAsync_ReturnsDto()
    {
        var article = new Article { Id = 2, Title = "A2", Content = "<p>x</p>" };
        _articleRepositoryMock.Setup(r => r.GetArticleByIdAsync(2)).ReturnsAsync(article);

        var result = await _articleService.GetArticleByIdAsync(2);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(article.Id));
    }

    [Test]
    public async Task GetAllArticlesAsync_ReturnsDtos()
    {
        var articles = new List<Article>
        {
            new Article { Id = 1, Title = "X" },
            new Article { Id = 2, Title = "Y" }
        };
        _articleRepositoryMock.Setup(r => r.GetAllArticlesAsync()).ReturnsAsync(articles);

        var result = await _articleService.GetAllArticlesAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.Any(d => d.Title == "X"));
    }

    [Test]
    public async Task UpdateArticleAsync_CallsRepositoryAndReturnsDto()
    {
        var dto = new ArticleDto { Id = 3, Title = "Up" };
        var article = new Article { Id = 3, Title = "Up" };
        _articleRepositoryMock.Setup(r => r.UpdateArticleAsync(It.IsAny<Article>())).ReturnsAsync(article);
        _articleRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _articleService.UpdateArticleAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(article.Id));
        _articleRepositoryMock.Verify(r => r.UpdateArticleAsync(It.Is<Article>(a => a.Id == dto.Id && a.Title == dto.Title)), Times.Once);
        _articleRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteArticleAsync_CallsRepository()
    {
        _articleRepositoryMock.Setup(r => r.DeleteArticleAsync(4)).Returns(Task.CompletedTask);
        _articleRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        await _articleService.DeleteArticleAsync(4);

        _articleRepositoryMock.Verify(r => r.DeleteArticleAsync(4), Times.Once);
        _articleRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task SaveChangesAsync_CallsRepositoryAndReturnsResult()
    {
        _articleRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _articleService.SaveChangesAsync();

        Assert.That(result, Is.True);
        _articleRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}