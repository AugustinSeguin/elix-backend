using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure;
using ElixBackend.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Tests.Infrastructure.Repository;

[TestFixture]
public class ResourceRepositoryTest
{
    private ElixDbContext _context;
    private ResourceRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ElixDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ElixDbContext(options);
        _repository = new ResourceRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddResourceAsync_AddsResourceAndReturnsIt()
    {
        var resource = new Resource
        {
            Name = "Res1",
            Localization = new Localization { Latitude = 1.0, Longitude = 1.0 },
            PhoneNumber = "123"
        };

        var result = await _repository.AddResourceAsync(resource);
        await _repository.SaveChangesAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Res1"));
        Assert.That(await _context.Resources.AnyAsync(r => r.Name == "Res1"), Is.True);
    }

    [Test]
    public async Task GetResourceByIdAsync_ReturnsResource()
    {
        var resource = new Resource
        {
            Name = "Res2",
            Localization = new Localization { Latitude = 2.0, Longitude = 2.0 }
        };
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        var result = await _repository.GetResourceByIdAsync(resource.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Res2"));
    }

    [Test]
    public async Task SearchByKeywordAsync_ReturnsMatchingResources()
    {
        var r1 = new Resource { Name = "Alpha", Localization = new Localization { Latitude = 0, Longitude = 0 } };
        var r2 = new Resource { Name = "Beta", Localization = new Localization { Latitude = 0, Longitude = 0 } };
        _context.Resources.AddRange(r1, r2);
        await _context.SaveChangesAsync();

        var result = await _repository.SearchByKeywordAsync("Alp");

        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("Alpha"));
    }

    [Test]
    public async Task SearchByKeywordAsync_IsCaseInsensitive()
    {
        var r1 = new Resource { Name = "Alpha", Localization = new Localization { Latitude = 0, Longitude = 0 } };
        _context.Resources.Add(r1);
        await _context.SaveChangesAsync();

        var result = await _repository.SearchByKeywordAsync("alpha");

        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("Alpha"));
    }

    [Test]
    public async Task SearchByLocalizationAsync_ReturnsNearbyResources()
    {
        var r1 = new Resource { Name = "Near", Localization = new Localization { Latitude = 45.0, Longitude = 5.0 } };
        var r2 = new Resource { Name = "Far", Localization = new Localization { Latitude = 50.0, Longitude = 10.0 } };
        _context.Resources.AddRange(r1, r2);
        await _context.SaveChangesAsync();

        var result = await _repository.SearchByLocalizationAsync(45.05, 5.05); // Within 0.1 range

        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("Near"));
    }

    [Test]
    public async Task SearchByKeywordAsync_IgnoresAccentsAndSpecialCharacters()
    {
        var r1 = new Resource { Name = "Hôpital Saint-Louis", Localization = new Localization { Latitude = 0, Longitude = 0 } };
        _context.Resources.Add(r1);
        await _context.SaveChangesAsync();

        // Recherche avec "hopital saint louis" (sans accents ni tiret)
        var result1 = await _repository.SearchByKeywordAsync("hopital saint louis");
        Assert.That(result1.Count(), Is.EqualTo(1));
        Assert.That(result1.First().Name, Is.EqualTo("Hôpital Saint-Louis"));

        // Recherche avec "saintlouis" (sans espace)
        var result2 = await _repository.SearchByKeywordAsync("saintlouis");
        Assert.That(result2.Count(), Is.EqualTo(1));
        
        // Recherche avec "HÔPITAL" (majuscules + accents)
        var result3 = await _repository.SearchByKeywordAsync("HÔPITAL");
        Assert.That(result3.Count(), Is.EqualTo(1));
    }
}
