using Microsoft.AspNetCore.Mvc;
using Moq;
using ElixBackend.API.Controllers;
using ElixBackend.Business.IService;
using ElixBackend.Business.DTO;

namespace ElixBackend.Tests.API.Controllers;

[TestFixture]
public class ArticleControllerTest
{
    private Mock<IArticleService> _articleServiceMock;
    private ArticleController _controller;

    [SetUp]
    public void SetUp()
    {
        _articleServiceMock = new Mock<IArticleService>();
        _controller = new ArticleController(_articleServiceMock.Object);
    }

    [Test]
    public async Task GetAllArticlesAsync_ReturnsOkWithArticles()
    {
        var articles = new List<ArticleDto>
        {
            new ArticleDto { Id = 1, Title = "A1" },
            new ArticleDto { Id = 2, Title = "A2" }
        };
        _articleServiceMock.Setup(s => s.GetAllArticlesAsync()).ReturnsAsync(articles);

        var result = await _controller.GetAllArticlesAsync();

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        var value = ok.Value as IEnumerable<ArticleDto>;
        Assert.That(value, Is.Not.Null);
        Assert.That(value!.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetArticleByIdAsync_ReturnsOkWhenFound()
    {
        var article = new ArticleDto { Id = 1, Title = "Found" };
        _articleServiceMock.Setup(s => s.GetArticleByIdAsync(1)).ReturnsAsync(article);

        var result = await _controller.GetArticleByIdAsync(1);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.EqualTo(article));
    }

    [Test]
    public async Task GetArticleByIdAsync_ReturnsNotFoundWhenMissing()
    {
        _articleServiceMock.Setup(s => s.GetArticleByIdAsync(99)).ReturnsAsync((ArticleDto?)null);

        var result = await _controller.GetArticleByIdAsync(99);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task AddArticleAsync_ReturnsOkWithCreatedArticle()
    {
        var toCreate = new ArticleDto { Title = "New" };
        var created = new ArticleDto { Id = 5, Title = "New" };
        _articleServiceMock.Setup(s => s.AddArticleAsync(toCreate)).ReturnsAsync(created);

        var result = await _controller.AddArticleAsync(toCreate);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.EqualTo(created));
    }

    [Test]
    public async Task UpdateArticleAsync_ReturnsBadRequestWhenIdMismatch()
    {
        var dto = new ArticleDto { Id = 2, Title = "X" };

        var result = await _controller.UpdateArticleAsync(1, dto);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateArticleAsync_ReturnsOkWhenUpdated()
    {
        var dto = new ArticleDto { Id = 3, Title = "Updated" };
        _articleServiceMock.Setup(s => s.UpdateArticleAsync(dto)).ReturnsAsync(dto);

        var result = await _controller.UpdateArticleAsync(3, dto);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.EqualTo(dto));
    }

    [Test]
    public async Task DeleteArticleAsync_ReturnsNoContent()
    {
        _articleServiceMock.Setup(s => s.DeleteArticleAsync(7)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteArticleAsync(7);

        Assert.That(result, Is.TypeOf<NoContentResult>());
    }
}