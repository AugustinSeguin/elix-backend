using System.Collections.Generic;
using System.Linq;
using ElixBackend.Business.DTO;
using ElixBackend.Business.Service;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ElixBackend.Tests.Business.Services;

[TestFixture]
public class CategoryServiceTest
{
    private Mock<ICategoryRepository> _categoryRepositoryMock;
    private CategoryService _categoryService;
    private Mock<ILogger<CategoryService>> _loggerMock;

    [SetUp]
    public void SetUp()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _loggerMock = new Mock<ILogger<CategoryService>>();
        _categoryService = new CategoryService(_categoryRepositoryMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task AddCategoryAsync_CallsRepositoryAndReturnsCategory()
    {
        var dto = new CategoryDto { Title = "cat", Description = "desc" };
        var category = new Category { Id = 1, Title = "cat", Description = "desc" };
        _categoryRepositoryMock.Setup(r => r.AddCategoryAsync(It.IsAny<Category>())).ReturnsAsync(category);
        _categoryRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _categoryService.AddCategoryAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo(category.Title));
        Assert.That(result.Description, Is.EqualTo(category.Description));
        _categoryRepositoryMock.Verify(r => r.AddCategoryAsync(It.Is<Category>(c => c.Title == dto.Title && c.Description == dto.Description)), Times.Once);
        _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetCategoryByIdAsync_ReturnsCategory()
    {
        var category = new Category { Id = 2, Title = "cat2", Description = "desc2" };
        _categoryRepositoryMock.Setup(r => r.GetCategoryByIdAsync(2)).ReturnsAsync(category);

        var result = await _categoryService.GetCategoryByIdAsync(2);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo(category.Title));
        Assert.That(result.Description, Is.EqualTo(category.Description));
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo(category.Title));
        _categoryRepositoryMock.Verify(r => r.UpdateCategoryAsync(It.IsAny<Category>()), Times.Once);
        _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteCategoryAsync_CallsRepositoryAndReturnsBool()
    {
        _categoryRepositoryMock.Setup(r => r.DeleteCategoryAsync(4)).Returns(Task.CompletedTask);
        _categoryRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _categoryService.DeleteCategoryAsync(4);

        Assert.That(result, Is.True);
        _categoryRepositoryMock.Verify(r => r.DeleteCategoryAsync(4), Times.Once);
        _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task AddCategoryAsync_WithImage_ReturnsCategoryWithImagePath()
    {
        var dto = new CategoryDto 
        { 
            Title = "cat_with_image", 
            Description = "desc",
            ImageMediaPath = "/temp/image123.jpg"
        };
        var category = new Category 
        { 
            Id = 5, 
            Title = "cat_with_image", 
            Description = "desc",
            ImageMediaPath = "/temp/image123.jpg"
        };
        _categoryRepositoryMock.Setup(r => r.AddCategoryAsync(It.IsAny<Category>())).ReturnsAsync(category);
        _categoryRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _categoryService.AddCategoryAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ImageMediaPath, Is.EqualTo(category.ImageMediaPath));
    }

    [Test]
    public async Task UpdateCategoryAsync_WithImage_ReturnsCategoryWithImagePath()
    {
        var dto = new CategoryDto 
        { 
            Id = 6, 
            Title = "updated_cat", 
            Description = "updated_desc",
            ImageMediaPath = "/temp/new_image456.jpg"
        };
        var category = new Category 
        { 
            Id = 6, 
            Title = "updated_cat", 
            Description = "updated_desc",
            ImageMediaPath = "/temp/new_image456.jpg"
        };
        _categoryRepositoryMock.Setup(r => r.UpdateCategoryAsync(It.IsAny<Category>())).ReturnsAsync(category);
        _categoryRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _categoryService.UpdateCategoryAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ImageMediaPath, Is.EqualTo(category.ImageMediaPath));
    }
}