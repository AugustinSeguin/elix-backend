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
public class ResourceControllerTest
{
    private Mock<IResourceService> _resourceServiceMock;
    private ResourceController _controller;

    [SetUp]
    public void SetUp()
    {
        _resourceServiceMock = new Mock<IResourceService>();
        _controller = new ResourceController(_resourceServiceMock.Object);

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
    public async Task Index_ReturnsViewWithAllResources()
    {
        // Arrange
        var resources = new List<ResourceDto>
        {
            new() { Id = 1, Name = "Resource 1", Localization = new LocalizationDto() },
            new() { Id = 2, Name = "Resource 2", Localization = new LocalizationDto() }
        };
        _resourceServiceMock.Setup(s => s.GetAllResourcesAsync()).ReturnsAsync(resources);

        // Act
        var result = await _controller.Index() as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<IEnumerable<ResourceDto>>());
        var model = result.Model as IEnumerable<ResourceDto>;
        Assert.That(model.Count(), Is.EqualTo(2));
        _resourceServiceMock.Verify(s => s.GetAllResourcesAsync(), Times.Once);
    }

    #endregion

    #region Create Tests

    [Test]
    public void Create_Get_ReturnsViewWithNewResource()
    {
        // Act
        var result = _controller.Create() as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<ResourceDto>());
        var model = result.Model as ResourceDto;
        Assert.That(model.Name, Is.EqualTo(""));
        Assert.That(model.Localization, Is.Not.Null);
    }

    [Test]
    public async Task Create_Post_ValidModel_RedirectsToIndex()
    {
        // Arrange
        var dto = new ResourceDto { Name = "New Resource", Localization = new LocalizationDto() };
        _resourceServiceMock.Setup(s => s.AddResourceAsync(dto)).ReturnsAsync(dto);

        // Act
        var result = await _controller.Create(dto) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _resourceServiceMock.Verify(s => s.AddResourceAsync(dto), Times.Once);
    }

    [Test]
    public async Task Create_Post_InvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var dto = new ResourceDto { Name = "", Localization = new LocalizationDto() };
        _controller.ModelState.AddModelError("Name", "Required");

        // Act
        var result = await _controller.Create(dto) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.EqualTo(dto));
        _resourceServiceMock.Verify(s => s.AddResourceAsync(It.IsAny<ResourceDto>()), Times.Never);
    }

    [Test]
    public async Task Create_Post_ServiceException_ReturnsViewWithError()
    {
        // Arrange
        var dto = new ResourceDto { Name = "New Resource", Localization = new LocalizationDto() };
        _resourceServiceMock.Setup(s => s.AddResourceAsync(dto)).ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.Create(dto) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.EqualTo(dto));
        Assert.That(_controller.ModelState.IsValid, Is.False);
    }

    #endregion

    #region Edit Tests

    [Test]
    public async Task Edit_Get_ReturnsViewWithResource()
    {
        // Arrange
        var dto = new ResourceDto { Id = 1, Name = "Resource 1", Localization = new LocalizationDto() };
        _resourceServiceMock.Setup(s => s.GetResourceByIdAsync(1)).ReturnsAsync(dto);

        // Act
        var result = await _controller.Edit(1) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.EqualTo(dto));
    }

    [Test]
    public async Task Edit_Post_ValidModel_RedirectsToIndex()
    {
        // Arrange
        var dto = new ResourceDto { Id = 1, Name = "Updated Resource", Localization = new LocalizationDto() };
        _resourceServiceMock.Setup(s => s.UpdateResourceAsync(dto)).ReturnsAsync(dto);

        // Act
        var result = await _controller.Edit(dto) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _resourceServiceMock.Verify(s => s.UpdateResourceAsync(dto), Times.Once);
    }

    [Test]
    public async Task Edit_Post_InvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var dto = new ResourceDto { Id = 1, Name = "", Localization = new LocalizationDto() };
        _controller.ModelState.AddModelError("Name", "Required");

        // Act
        var result = await _controller.Edit(dto) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.EqualTo(dto));
        _resourceServiceMock.Verify(s => s.UpdateResourceAsync(It.IsAny<ResourceDto>()), Times.Never);
    }

    [Test]
    public async Task Edit_Post_ServiceException_ReturnsViewWithError()
    {
        // Arrange
        var dto = new ResourceDto { Id = 1, Name = "Updated Resource", Localization = new LocalizationDto() };
        _resourceServiceMock.Setup(s => s.UpdateResourceAsync(dto)).ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.Edit(dto) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.EqualTo(dto));
        Assert.That(_controller.ModelState.IsValid, Is.False);
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task Delete_CallsServiceAndRedirectsToIndex()
    {
        // Arrange
        int id = 1;
        _resourceServiceMock.Setup(s => s.DeleteResourceAsync(id)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(id) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _resourceServiceMock.Verify(s => s.DeleteResourceAsync(id), Times.Once);
    }

    [Test]
    public async Task Delete_ServiceException_RedirectsToIndex()
    {
        // Arrange
        int id = 1;
        _resourceServiceMock.Setup(s => s.DeleteResourceAsync(id)).ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.Delete(id) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _resourceServiceMock.Verify(s => s.DeleteResourceAsync(id), Times.Once);
    }

    #endregion
}
