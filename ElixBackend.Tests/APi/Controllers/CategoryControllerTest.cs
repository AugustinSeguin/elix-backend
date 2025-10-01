using ElixBackend.API.Controllers;
using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ElixBackend.Tests.APi.Controllers;

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
    }

    [Test]
    public async Task GetAll_ReturnsOkWithCategories()
    {
        var categories = new List<CategoryDto>
        {
            new CategoryDto { Id = 1, Title = "A", Description = "descA" },
            new CategoryDto { Id = 2, Title = "B", Description = "descB" }
        };
        _categoryServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);

        var result = await _controller.GetAll();

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = result.Result as OkObjectResult;
        Assert.That(ok.Value, Is.EqualTo(categories));
    }

    [Test]
    public async Task GetById_ReturnsOkIfFound()
    {
        var category = new CategoryDto { Id = 1, Title = "A", Description = "descA" };
        _categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(1)).ReturnsAsync(new Domain.Entities.Category { Id = 1, Title = "A", Description = "descA" });

        var result = await _controller.GetById(1);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        if (result.Result is OkObjectResult ok) Assert.That(ok.Value, Is.Not.Null);
    }

    [Test]
    public async Task GetById_ReturnsNotFoundIfNull()
    {
        _categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(99)).ReturnsAsync((Domain.Entities.Category?)null);

        var result = await _controller.GetById(99);

        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var dto = new CategoryDto { Title = "C", Description = "descC" };
        var created = new Domain.Entities.Category { Id = 3, Title = "C", Description = "descC" };
        _categoryServiceMock.Setup(s => s.AddCategoryAsync(dto)).ReturnsAsync(created);

        var result = await _controller.Create(dto);

        Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
        var createdResult = result.Result as CreatedAtActionResult;
        if (createdResult != null) Assert.That(createdResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task Update_ReturnsOkIfValid()
    {
        var dto = new CategoryDto { Id = 4, Title = "D", Description = "descD" };
        var updated = new Domain.Entities.Category { Id = 4, Title = "D", Description = "descD" };
        _categoryServiceMock.Setup(s => s.UpdateCategoryAsync(dto)).ReturnsAsync(updated);

        var result = await _controller.Update(4, dto);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = result.Result as OkObjectResult;
        Assert.That(ok.Value, Is.Not.Null);
    }

    [Test]
    public async Task Update_ReturnsBadRequestIfIdMismatch()
    {
        var dto = new CategoryDto { Id = 5, Title = "E", Description = "descE" };

        var result = await _controller.Update(6, dto);

        Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
    }

    [Test]
    public async Task Delete_ReturnsNoContent()
    {
        _categoryServiceMock.Setup(s => s.DeleteCategoryAsync(7)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(7);

        Assert.That(result, Is.TypeOf<NoContentResult>());
    }
}