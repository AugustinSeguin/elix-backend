using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.WebApp.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace ElixBackend.Tests.WebApp.Controllers;

[TestFixture]
public class ArticleControllerTest
{
    private Mock<IArticleService> _articleServiceMock;
    private Mock<ICategoryService> _categoryServiceMock;
    private Mock<IConfiguration> _configurationMock;
    private ArticleController _controller;

    [SetUp]
    public void SetUp()
    {
        _articleServiceMock = new Mock<IArticleService>();
        _categoryServiceMock = new Mock<ICategoryService>();
        _configurationMock = new Mock<IConfiguration>();
        
        // Setup configuration
        _configurationMock.Setup(c => c["FileStorage:UploadsPath"]).Returns("wwwroot/uploads");
        _configurationMock.Setup(c => c["FileStorage:UploadsUrlPath"]).Returns("/uploads");
        
        _controller = new ArticleController(_articleServiceMock.Object, _categoryServiceMock.Object, _configurationMock.Object);
        
        // Setup TempData and HttpContext
        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    #region Index Tests

    [Test]
    public async Task Index_ReturnsViewWithAllArticles()
    {
        // Arrange
        var articles = new List<ArticleDto>
        {
            new() { Id = 1, Title = "Article 1", Content = "Content 1", CategoryId = 1 },
            new() { Id = 2, Title = "Article 2", Content = "Content 2", CategoryId = 2 },
            new() { Id = 3, Title = "Article 3", Content = "Content 3", CategoryId = 1 }
        };
        _articleServiceMock.Setup(s => s.GetAllArticlesAsync()).ReturnsAsync(articles);

        // Act
        var result = await _controller.Index() as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<IEnumerable<ArticleDto>>());
        var model = result.Model as IEnumerable<ArticleDto>;
        Assert.That(model.Count(), Is.EqualTo(3));
        _articleServiceMock.Verify(s => s.GetAllArticlesAsync(), Times.Once);
    }

    [Test]
    public async Task Index_WithEmptyList_ReturnsViewWithEmptyList()
    {
        // Arrange
        var articles = new List<ArticleDto>();
        _articleServiceMock.Setup(s => s.GetAllArticlesAsync()).ReturnsAsync(articles);

        // Act
        var result = await _controller.Index() as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        var model = result.Model as IEnumerable<ArticleDto>;
        Assert.That(model.Count(), Is.EqualTo(0));
    }

    #endregion

    #region Create GET Tests

    [Test]
    public async Task Create_Get_ReturnsViewWithNewArticle()
    {
        // Arrange
        var categories = new List<CategoryDto>
        {
            new() { Id = 1, Title = "Category 1" },
            new() { Id = 2, Title = "Category 2" }
        };
        _categoryServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await _controller.Create() as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<ArticleDto>());
        var model = result.Model as ArticleDto;
        Assert.That(model.Id, Is.EqualTo(0));
        Assert.That(model.Title, Is.EqualTo(""));
        _categoryServiceMock.Verify(s => s.GetAllCategoriesAsync(), Times.Once);
    }

    #endregion

    #region Create POST Tests

    [Test]
    public async Task Create_Post_WithValidModelNoFile_RedirectsToIndex()
    {
        // Arrange
        var articleDto = new ArticleDto { Title = "New Article", Content = "Content", CategoryId = 1 };
        var categories = new List<CategoryDto> { new() { Id = 1, Title = "Category 1" } };
        _categoryServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);
        _articleServiceMock.Setup(s => s.AddArticleAsync(It.IsAny<ArticleDto>()))
            .ReturnsAsync(new ArticleDto { Id = 1, Title = "New Article", Content = "Content", CategoryId = 1 });

        // Act
        var result = await _controller.Create(articleDto, null) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _articleServiceMock.Verify(s => s.AddArticleAsync(It.Is<ArticleDto>(a => a.Title == "New Article")), Times.Once);
    }

    [Test]
    public async Task Create_Post_WithInvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var articleDto = new ArticleDto { Title = "", Content = "Content", CategoryId = 1 };
        var categories = new List<CategoryDto> { new() { Id = 1, Title = "Category 1" } };
        _categoryServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);
        _controller.ModelState.AddModelError("Title", "Le titre est requis");

        // Act
        var result = await _controller.Create(articleDto, null) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<ArticleDto>());
        var model = result.Model as ArticleDto;
        Assert.That(model.Title, Is.EqualTo(""));
        _articleServiceMock.Verify(s => s.AddArticleAsync(It.IsAny<ArticleDto>()), Times.Never);
    }

    [Test]
    public async Task Create_Post_ServiceThrowsException_ReturnsViewWithError()
    {
        // Arrange
        var articleDto = new ArticleDto { Title = "Article", Content = "Content", CategoryId = 1 };
        var categories = new List<CategoryDto> { new() { Id = 1, Title = "Category 1" } };
        _categoryServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);
        _articleServiceMock.Setup(s => s.AddArticleAsync(It.IsAny<ArticleDto>())).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Create(articleDto, null) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<ArticleDto>());
        Assert.That(_controller.ModelState.ErrorCount, Is.GreaterThan(0));
    }

    #endregion

    #region Edit GET Tests

    [Test]
    public async Task Edit_Get_WithValidId_ReturnsViewWithArticle()
    {
        // Arrange
        var articleId = 5;
        var article = new ArticleDto { Id = articleId, Title = "Article", Content = "Content", CategoryId = 1 };
        var categories = new List<CategoryDto> { new() { Id = 1, Title = "Category 1" } };
        _articleServiceMock.Setup(s => s.GetArticleByIdAsync(articleId)).ReturnsAsync(article);
        _categoryServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await _controller.Edit(articleId) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<ArticleDto>());
        var model = result.Model as ArticleDto;
        Assert.That(model.Id, Is.EqualTo(articleId));
        Assert.That(model.Title, Is.EqualTo("Article"));
        _articleServiceMock.Verify(s => s.GetArticleByIdAsync(articleId), Times.Once);
        _categoryServiceMock.Verify(s => s.GetAllCategoriesAsync(), Times.Once);
    }

    [Test]
    public async Task Edit_Get_WithNullArticle_ReturnsNotFound()
    {
        // Arrange
        var articleId = 999;
        _articleServiceMock.Setup(s => s.GetArticleByIdAsync(articleId)).ReturnsAsync((ArticleDto?)null);

        // Act
        var result = await _controller.Edit(articleId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
        _categoryServiceMock.Verify(s => s.GetAllCategoriesAsync(), Times.Never);
    }

    #endregion

    #region Edit POST Tests

    [Test]
    public async Task Edit_Post_WithValidModelNoFile_RedirectsToIndex()
    {
        // Arrange
        var articleDto = new ArticleDto { Id = 5, Title = "Updated Article", Content = "Updated Content", CategoryId = 1 };
        var existingArticle = new ArticleDto { Id = 5, Title = "Old Article", Content = "Old Content", CategoryId = 1, MediaPath = "/uploads/old.jpg" };
        var categories = new List<CategoryDto> { new() { Id = 1, Title = "Category 1" } };
        
        _articleServiceMock.Setup(s => s.GetArticleByIdAsync(5)).ReturnsAsync(existingArticle);
        _categoryServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);
        _articleServiceMock.Setup(s => s.UpdateArticleAsync(It.IsAny<ArticleDto>()))
            .ReturnsAsync(new ArticleDto { Id = 5, Title = "Updated Article", Content = "Updated Content", CategoryId = 1 });

        // Act
        var result = await _controller.Edit(articleDto, null) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _articleServiceMock.Verify(s => s.UpdateArticleAsync(It.Is<ArticleDto>(a => a.Id == 5 && a.Title == "Updated Article")), Times.Once);
    }

    [Test]
    public async Task Edit_Post_WithInvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var articleDto = new ArticleDto { Id = 5, Title = "", Content = "Content", CategoryId = 1 };
        var categories = new List<CategoryDto> { new() { Id = 1, Title = "Category 1" } };
        _categoryServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);
        _controller.ModelState.AddModelError("Title", "Le titre est requis");

        // Act
        var result = await _controller.Edit(articleDto, null) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<ArticleDto>());
        var model = result.Model as ArticleDto;
        Assert.That(model.Id, Is.EqualTo(5));
        _articleServiceMock.Verify(s => s.UpdateArticleAsync(It.IsAny<ArticleDto>()), Times.Never);
    }

    [Test]
    public async Task Edit_Post_ServiceThrowsException_ReturnsViewWithError()
    {
        // Arrange
        var articleDto = new ArticleDto { Id = 5, Title = "Article", Content = "Content", CategoryId = 1 };
        var existingArticle = new ArticleDto { Id = 5, Title = "Old Article", Content = "Old Content", CategoryId = 1 };
        var categories = new List<CategoryDto> { new() { Id = 1, Title = "Category 1" } };
        
        _articleServiceMock.Setup(s => s.GetArticleByIdAsync(5)).ReturnsAsync(existingArticle);
        _categoryServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);
        _articleServiceMock.Setup(s => s.UpdateArticleAsync(It.IsAny<ArticleDto>())).ThrowsAsync(new Exception("Update failed"));

        // Act
        var result = await _controller.Edit(articleDto, null) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<ArticleDto>());
        Assert.That(_controller.ModelState.ErrorCount, Is.GreaterThan(0));
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task Delete_WithValidId_RedirectsToIndex()
    {
        // Arrange
        var articleId = 5;
        _articleServiceMock.Setup(s => s.DeleteArticleAsync(articleId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(articleId) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _articleServiceMock.Verify(s => s.DeleteArticleAsync(articleId), Times.Once);
    }

    [Test]
    public async Task Delete_ServiceThrowsException_RedirectsToIndex()
    {
        // Arrange
        var articleId = 5;
        _articleServiceMock.Setup(s => s.DeleteArticleAsync(articleId)).ThrowsAsync(new Exception("Delete failed"));

        // Act
        var result = await _controller.Delete(articleId) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _articleServiceMock.Verify(s => s.DeleteArticleAsync(articleId), Times.Once);
    }

    [Test]
    public async Task Delete_WithNonExistingArticle_StillRedirectsToIndex()
    {
        // Arrange
        var articleId = 999;
        _articleServiceMock.Setup(s => s.DeleteArticleAsync(articleId)).ThrowsAsync(new Exception("Article not found"));

        // Act
        var result = await _controller.Delete(articleId) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
    }

    #endregion
}