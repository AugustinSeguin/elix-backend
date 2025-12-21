using ElixBackend.Business.DTO;
using ElixBackend.Business.Service;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ElixBackend.Tests.Business.Services;

[TestFixture]
public class ResourceServiceTest
{
    private Mock<IResourceRepository> _resourceRepositoryMock;
    private ResourceService _resourceService;
    private Mock<ILogger<ResourceService>> _loggerMock;

    [SetUp]
    public void SetUp()
    {
        _resourceRepositoryMock = new Mock<IResourceRepository>();
        _loggerMock = new Mock<ILogger<ResourceService>>();
        _resourceService = new ResourceService(_resourceRepositoryMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task AddResourceAsync_CallsRepositoryAndReturnsResource()
    {
        var dto = new ResourceDto
        {
            Name = "Res1",
            Localization = new LocalizationDto { Latitude = 1.0, Longitude = 1.0 },
            PhoneNumber = "123"
        };
        var resource = new Resource
        {
            Id = 1,
            Name = "Res1",
            Localization = new Localization { Latitude = 1.0, Longitude = 1.0 },
            PhoneNumber = "123"
        };
        _resourceRepositoryMock.Setup(r => r.AddResourceAsync(It.IsAny<Resource>())).ReturnsAsync(resource);
        _resourceRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _resourceService.AddResourceAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo(resource.Name));
        _resourceRepositoryMock.Verify(r => r.AddResourceAsync(It.IsAny<Resource>()), Times.Once);
        _resourceRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetResourceByIdAsync_ReturnsResource()
    {
        var resource = new Resource
        {
            Id = 2,
            Name = "Res2",
            Localization = new Localization { Latitude = 2.0, Longitude = 2.0 }
        };
        _resourceRepositoryMock.Setup(r => r.GetResourceByIdAsync(2)).ReturnsAsync(resource);

        var result = await _resourceService.GetResourceByIdAsync(2);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Res2"));
    }

    [Test]
    public async Task SearchByKeywordAsync_ReturnsMatchingResources()
    {
        var resources = new List<Resource>
        {
            new Resource { Name = "Alpha", Localization = new Localization { Latitude = 0, Longitude = 0 } }
        };
        _resourceRepositoryMock.Setup(r => r.SearchByKeywordAsync("Alp")).ReturnsAsync(resources);

        var result = await _resourceService.SearchByKeywordAsync("Alp");

        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("Alpha"));
    }

    [Test]
    public async Task SearchByLocalizationAsync_ReturnsNearbyResources()
    {
        var resources = new List<Resource>
        {
            new Resource { Name = "Near", Localization = new Localization { Latitude = 45.0, Longitude = 5.0 } }
        };
        _resourceRepositoryMock.Setup(r => r.SearchByLocalizationAsync(45.05, 5.05)).ReturnsAsync(resources);

        var result = await _resourceService.SearchByLocalizationAsync(45.05, 5.05);

        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("Near"));
    }
}
