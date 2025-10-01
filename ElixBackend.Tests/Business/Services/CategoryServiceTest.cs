using ElixBackend.Business.DTO;
using ElixBackend.Business.Service;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Moq;

namespace ElixBackend.Tests.Business.Services;

[TestFixture]
public class CategoryServiceTest
{
    private Mock<ICategoryRepository> _categoryRepositoryMock;
    private CategoryService _categoryService;

    [SetUp]
    public void SetUp()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _categoryService = new CategoryService(_categoryRepositoryMock.Object);
    }

    [Test]
    public async Task AddCategoryAsync_CallsRepositoryAndReturnsCategory()
    {
        var dto = new CategoryDto { Title = "cat", Description = "desc" };
        var category = new Category { Id = 1, Title = "cat", Description = "desc" };
        _categoryRepositoryMock.Setup(r => r.AddCategoryAsync(It.IsAny<Category>())).ReturnsAsync(category);
        _categoryRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _categoryService.AddCategoryAsync(dto);

        Assert.That(result, Is.EqualTo(category));
        _categoryRepositoryMock.Verify(r => r.AddCategoryAsync(It.Is<Category>(c => c.Title == dto.Title && c.Description == dto.Description)), Times.Once);
        _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetCategoryByIdAsync_ReturnsCategory()
    {
        var category = new Category { Id = 2, Title = "cat2", Description = "desc2" };
        _categoryRepositoryMock.Setup(r => r.GetCategoryByIdAsync(2)).ReturnsAsync(category);

        var result = await _categoryService.GetCategoryByIdAsync(2);

        Assert.That(result, Is.EqualTo(category));
    }

    [Test]
    public async Task GetAllCategoriesAsync_ReturnsCategoryDtos()
    {
        var categories = new List<Category>
        {
            new Category { Id = 1, Title = "A", Description = "A" },
            new Category { Id = 2, Title = "B", Description = "B" }
        };
        _categoryRepositoryMock.Setup(r => r.GetAllCategoriesAsync()).ReturnsAsync(categories);

        var result = await _categoryService.GetAllCategoriesAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.Any(dto => dto.Title == "A"), Is.True);
        Assert.That(result.Any(dto => dto.Title == "B"), Is.True);
    }

    [Test]
    public async Task UpdateCategoryAsync_CallsRepositoryAndReturnsCategory()
    {
        var dto = new CategoryDto { Id = 3, Title = "up", Description = "updesc" };
        var category = new Category { Id = 3, Title = "up", Description = "updesc" };
        _categoryRepositoryMock.Setup(r => r.UpdateCategoryAsync(It.IsAny<Category>())).ReturnsAsync(category);
        _categoryRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _categoryService.UpdateCategoryAsync(dto);

        Assert.That(result, Is.EqualTo(category));
        _categoryRepositoryMock.Verify(r => r.UpdateCategoryAsync(It.Is<Category>(c => c.Id == dto.Id && c.Title == dto.Title)), Times.Once);
        _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteCategoryAsync_CallsRepository()
    {
        _categoryRepositoryMock.Setup(r => r.DeleteCategoryAsync(4)).Returns(Task.CompletedTask);
        _categoryRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        await _categoryService.DeleteCategoryAsync(4);

        _categoryRepositoryMock.Verify(r => r.DeleteCategoryAsync(4), Times.Once);
        _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task SaveChangesAsync_CallsRepositoryAndReturnsResult()
    {
        _categoryRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _categoryService.SaveChangesAsync();

        Assert.That(result, Is.True);
        _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}