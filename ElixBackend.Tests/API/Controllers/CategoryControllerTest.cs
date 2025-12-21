using ElixBackend.API.Controllers;
using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace ElixBackend.Tests.API.Controllers;


[TestFixture]
public class CategoryControllerTest
{
    private Mock<ICategoryService> _categoryServiceMock;
    private CategoryController _controller;

    [SetUp]
    public void SetUp()
    {
        _categoryServiceMock = new Mock<ICategoryService>();
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["FileStorage:UploadsPath"]).Returns("./temp");
        _controller = new CategoryController(_categoryServiceMock.Object, configMock.Object);
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
        Assert.That(ok, Is.Not.Null);
        var value = ok!.Value as IEnumerable<CategoryDto>;
        Assert.That(value, Is.Not.Null);
        Assert.That(value!.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetById_ReturnsOkIfFound()
    {
        var dto = new CategoryDto { Id = 1, Title = "A", Description = "descA" };
        _categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(1)).ReturnsAsync(dto);

        var result = await _controller.GetById(1);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        if (result.Result is OkObjectResult ok) {
            var val = ok.Value as CategoryDto;
            Assert.That(val, Is.Not.Null);
            Assert.That(val!.Id, Is.EqualTo(dto.Id));
        }
    }

    [Test]
    public async Task GetById_ReturnsNotFoundIfNull()
    {
        _categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(99)).ReturnsAsync((CategoryDto?)null);

        var result = await _controller.GetById(99);

        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var dto = new CategoryDto { Title = "C", Description = "descC" };
        var created = new CategoryDto { Id = 3, Title = "C", Description = "descC" };
        _categoryServiceMock.Setup(s => s.AddCategoryAsync(dto)).ReturnsAsync(created);

        var result = await _controller.Create(dto);

        Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        var val = createdResult!.Value as CategoryDto;
        Assert.That(val, Is.Not.Null);
        Assert.That(val!.Id, Is.EqualTo(created.Id));
    }

    [Test]
    public async Task Update_ReturnsOkIfValid()
    {
        var dto = new CategoryDto { Id = 4, Title = "D", Description = "descD" };
        var updated = new CategoryDto { Id = 4, Title = "D", Description = "descD" };
        
        // Mock GetCategoryByIdAsync pour que la catégorie soit trouvée
        _categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(4)).ReturnsAsync(dto);
        _categoryServiceMock.Setup(s => s.UpdateCategoryAsync(dto)).ReturnsAsync(updated);

        var result = await _controller.Update(4, dto);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var val = ok!.Value as CategoryDto;
        Assert.That(val, Is.Not.Null);
        Assert.That(val!.Id, Is.EqualTo(updated.Id));
    }

    [Test]
    public async Task Update_ReturnsBadRequest_WhenIdMismatch()
    {
        var dto = new CategoryDto { Id = 4, Title = "D", Description = "descD" };

        var result = await _controller.Update(5, dto);

        Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
    }

    [Test]
    public async Task Update_ReturnsNotFound_WhenCategoryDoesNotExist()
    {
        var dto = new CategoryDto { Id = 99, Title = "D", Description = "descD" };
        _categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(99)).ReturnsAsync((CategoryDto?)null);

        var result = await _controller.Update(99, dto);

        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_ReturnsNoContent()
    {
        _categoryServiceMock.Setup(s => s.DeleteCategoryAsync(1)).ReturnsAsync(true);

        var result = await _controller.Delete(1);

        Assert.That(result, Is.TypeOf<NoContentResult>());
    }

    [Test]
    public async Task Delete_ReturnsProblem_WhenFailed()
    {
        _categoryServiceMock.Setup(s => s.DeleteCategoryAsync(99)).ReturnsAsync((bool?)null);

        var result = await _controller.Delete(99);

        Assert.That(result, Is.TypeOf<ObjectResult>());
        var objResult = result as ObjectResult;
        Assert.That(objResult!.StatusCode, Is.EqualTo(500));
    }
}