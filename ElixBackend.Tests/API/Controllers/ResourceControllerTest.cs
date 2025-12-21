using ElixBackend.API.Controllers;
using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace ElixBackend.Tests.API.Controllers;

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
    }

    [Test]
    public async Task GetAll_ReturnsOkWithResources()
    {
        var resources = new List<ResourceDto>
        {
            new ResourceDto { Id = 1, Name = "A", Localization = new LocalizationDto() },
            new ResourceDto { Id = 2, Name = "B", Localization = new LocalizationDto() }
        };
        _resourceServiceMock.Setup(s => s.GetAllResourcesAsync()).ReturnsAsync(resources);

        var result = await _controller.GetAll();

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var value = ok!.Value as IEnumerable<ResourceDto>;
        Assert.That(value, Is.Not.Null);
        Assert.That(value!.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetById_ReturnsOkIfFound()
    {
        var dto = new ResourceDto { Id = 1, Name = "A", Localization = new LocalizationDto() };
        _resourceServiceMock.Setup(s => s.GetResourceByIdAsync(1)).ReturnsAsync(dto);

        var result = await _controller.GetById(1);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = result.Result as OkObjectResult;
        Assert.That(ok!.Value, Is.EqualTo(dto));
    }

    [Test]
    public async Task Create_ReturnsCreated()
    {
        var dto = new ResourceDto { Name = "A", Localization = new LocalizationDto() };
        var createdDto = new ResourceDto { Id = 1, Name = "A", Localization = new LocalizationDto() };
        _resourceServiceMock.Setup(s => s.AddResourceAsync(dto)).ReturnsAsync(createdDto);

        var result = await _controller.Create(dto);

        Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
        var created = result.Result as CreatedAtActionResult;
        Assert.That(created!.Value, Is.EqualTo(createdDto));
    }

    [Test]
    public async Task SearchByKeyword_ReturnsOk()
    {
        var resources = new List<ResourceDto>
        {
            new ResourceDto { Id = 1, Name = "Alpha", Localization = new LocalizationDto() }
        };
        _resourceServiceMock.Setup(s => s.SearchByKeywordAsync("Alp")).ReturnsAsync(resources);

        var result = await _controller.SearchByKeyword("Alp");

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = result.Result as OkObjectResult;
        var value = ok!.Value as IEnumerable<ResourceDto>;
        Assert.That(value!.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task SearchByLocalization_ReturnsOk()
    {
        var resources = new List<ResourceDto>
        {
            new ResourceDto { Id = 1, Name = "Near", Localization = new LocalizationDto() }
        };
        _resourceServiceMock.Setup(s => s.SearchByLocalizationAsync(45.0, 5.0)).ReturnsAsync(resources);

        var result = await _controller.SearchByLocalization(45.0, 5.0);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = result.Result as OkObjectResult;
        var value = ok!.Value as IEnumerable<ResourceDto>;
        Assert.That(value!.Count(), Is.EqualTo(1));
    }
}
