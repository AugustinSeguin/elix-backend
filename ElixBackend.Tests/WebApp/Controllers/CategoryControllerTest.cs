using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.WebApp.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;

namespace ElixBackend.Tests.WebApp.Controllers;

[TestFixture]
public class CategoryControllerTest
{
    private Mock<ICategoryService> _categoryServiceMock;
    private CategoryController _controller;

    [SetUp]
    public void SetUp()
    {
        _categoryServiceMock = new Mock<ICategoryService>();
        _controller = new CategoryController(_categoryServiceMock.Object);
        
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
    public async Task Index_ReturnsViewWithAllCategories()
    {
        // Arrange
        var categories = new List<CategoryDto>
        {
            new() { Id = 1, Title = "Category 1", Description = "Description 1" },
            new() { Id = 2, Title = "Category 2", Description = "Description 2" },
            new() { Id = 3, Title = "Category 3", Description = "Description 3" }
        };
        _categoryServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await _controller.Index() as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<IEnumerable<CategoryDto>>());
        var model = result.Model as IEnumerable<CategoryDto>;
        Assert.That(model.Count(), Is.EqualTo(3));
        _categoryServiceMock.Verify(s => s.GetAllCategoriesAsync(), Times.Once);
    }

    [Test]
    public async Task Index_WithEmptyList_ReturnsViewWithEmptyList()
    {
        // Arrange
        var categories = new List<CategoryDto>();
        _categoryServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await _controller.Index() as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        var model = result.Model as IEnumerable<CategoryDto>;
        Assert.That(model.Count(), Is.EqualTo(0));
    }

    #endregion

    #region Create GET Tests

    [Test]
    public void Create_Get_ReturnsViewWithNewCategory()
    {
        // Act
        var result = _controller.Create() as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<CategoryDto>());
        var model = result.Model as CategoryDto;
        Assert.That(model.Id, Is.EqualTo(0));
        Assert.That(model.Title, Is.EqualTo(""));
    }

    #endregion

    #region Create POST Tests

    [Test]
    public async Task Create_Post_WithValidModel_RedirectsToIndex()
    {
        // Arrange
        var categoryDto = new CategoryDto { Title = "New Category", Description = "New Description" };
         _categoryServiceMock.Setup(s => s.AddCategoryAsync(It.IsAny<CategoryDto>()))
            .ReturnsAsync(new CategoryDto { Id = 1, Title = "New Category", Description = "New Description" });

        // Act
        var result = await _controller.Create(categoryDto) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _categoryServiceMock.Verify(s => s.AddCategoryAsync(It.Is<CategoryDto>(c => c.Title == "New Category")), Times.Once);
    }

    [Test]
    public async Task Create_Post_WithInvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var categoryDto = new CategoryDto { Title = "", Description = "Description" };
        _controller.ModelState.AddModelError("Title", "Le titre est requis");

        // Act
        var result = await _controller.Create(categoryDto) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<CategoryDto>());
        var model = result.Model as CategoryDto;
        Assert.That(model.Title, Is.EqualTo(""));
        _categoryServiceMock.Verify(s => s.AddCategoryAsync(It.IsAny<CategoryDto>()), Times.Never);
    }

    [Test]
    public async Task Create_Post_ServiceThrowsException_ReturnsViewWithError()
    {
        // Arrange
        var categoryDto = new CategoryDto { Title = "Category", Description = "Description" };
        _categoryServiceMock.Setup(s => s.AddCategoryAsync(It.IsAny<CategoryDto>())).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Create(categoryDto) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<CategoryDto>());
        Assert.That(_controller.ModelState.ErrorCount, Is.GreaterThan(0));
        Assert.That(_controller.ModelState[""]?.Errors[0].ErrorMessage, Is.EqualTo("Impossible de créer la catégorie. Veuillez réessayer."));
    }

    #endregion

    #region Edit GET Tests

    [Test]
    public async Task Edit_Get_WithValidId_ReturnsViewWithCategory()
    {
        // Arrange
        var categoryId = 5;
        var category = new CategoryDto { Id = categoryId, Title = "Category", Description = "Description" };
        _categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(categoryId)).ReturnsAsync(category);

        // Act
        var result = await _controller.Edit(categoryId) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<CategoryDto>());
        var model = result.Model as CategoryDto;
        Assert.That(model.Id, Is.EqualTo(categoryId));
        Assert.That(model.Title, Is.EqualTo("Category"));
        _categoryServiceMock.Verify(s => s.GetCategoryByIdAsync(categoryId), Times.Once);
    }

    [Test]
    public async Task Edit_Get_WithNullCategory_ReturnsViewWithNull()
    {
        // Arrange
        var categoryId = 999;
        _categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(categoryId)).ReturnsAsync((CategoryDto?)null);

        // Act
        var result = await _controller.Edit(categoryId) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.Null);
        _categoryServiceMock.Verify(s => s.GetCategoryByIdAsync(categoryId), Times.Once);
    }

    #endregion

    #region Edit POST Tests

    [Test]
    public async Task Edit_Post_WithValidModel_RedirectsToIndex()
    {
        // Arrange
        var categoryDto = new CategoryDto { Id = 5, Title = "Updated Category", Description = "Updated Description" };
        _categoryServiceMock.Setup(s => s.UpdateCategoryAsync(It.IsAny<CategoryDto>()))
            .ReturnsAsync(new CategoryDto { Id = 5, Title = "Updated Category", Description = "Updated Description" });

        // Act
        var result = await _controller.Edit(categoryDto) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _categoryServiceMock.Verify(s => s.UpdateCategoryAsync(It.Is<CategoryDto>(c => c.Id == 5 && c.Title == "Updated Category")), Times.Once);
    }

    [Test]
    public async Task Edit_Post_WithInvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var categoryDto = new CategoryDto { Id = 5, Title = "", Description = "Description" };
        _controller.ModelState.AddModelError("Title", "Le titre est requis");

        // Act
        var result = await _controller.Edit(categoryDto) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<CategoryDto>());
        var model = result.Model as CategoryDto;
        Assert.That(model.Id, Is.EqualTo(5));
        _categoryServiceMock.Verify(s => s.UpdateCategoryAsync(It.IsAny<CategoryDto>()), Times.Never);
    }

    [Test]
    public async Task Edit_Post_ServiceThrowsException_ReturnsViewWithError()
    {
        // Arrange
        var categoryDto = new CategoryDto { Id = 5, Title = "Category", Description = "Description" };
        _categoryServiceMock.Setup(s => s.UpdateCategoryAsync(It.IsAny<CategoryDto>())).ThrowsAsync(new Exception("Update failed"));

        // Act
        var result = await _controller.Edit(categoryDto) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<CategoryDto>());
        Assert.That(_controller.ModelState.ErrorCount, Is.GreaterThan(0));
        Assert.That(_controller.ModelState[""]?.Errors[0].ErrorMessage, Is.EqualTo("Impossible de mettre à jour la catégorie. Veuillez réessayer."));
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task Delete_WithValidId_RedirectsToIndex()
    {
        // Arrange
        var categoryId = 5;
        _categoryServiceMock.Setup(s => s.DeleteCategoryAsync(categoryId)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(categoryId) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _categoryServiceMock.Verify(s => s.DeleteCategoryAsync(categoryId), Times.Once);
    }

    [Test]
    public async Task Delete_ServiceThrowsException_RedirectsToIndex()
    {
        // Arrange
        var categoryId = 5;
        _categoryServiceMock.Setup(s => s.DeleteCategoryAsync(categoryId)).ThrowsAsync(new Exception("Delete failed"));

        // Act
        var result = await _controller.Delete(categoryId) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _categoryServiceMock.Verify(s => s.DeleteCategoryAsync(categoryId), Times.Once);
    }

    [Test]
    public async Task Delete_WithNonExistingCategory_StillRedirectsToIndex()
    {
        // Arrange
        var categoryId = 999;
        _categoryServiceMock.Setup(s => s.DeleteCategoryAsync(categoryId)).ThrowsAsync(new Exception("Category not found"));

        // Act
        var result = await _controller.Delete(categoryId) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
    }

    #endregion
}