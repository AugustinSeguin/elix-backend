using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure;
using ElixBackend.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Tests.Infrastructure.Repository;

[TestFixture]
public class CategoryRepositoryTest
{
    private ElixDbContext _context;
    private CategoryRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ElixDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ElixDbContext(options);
        _repository = new CategoryRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddCategoryAsync_AddsCategoryAndReturnsIt()
    {
        var category = new Category { Title = "Cat1", Description = "Desc1" };

        var result = await _repository.AddCategoryAsync(category);
        await _repository.SaveChangesAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Cat1"));
        Assert.That(await _context.Categories.AnyAsync(c => c.Title == "Cat1"), Is.True);
    }

    [Test]
    public async Task GetCategoryByIdAsync_ReturnsCategory()
    {
        var category = new Category { Title = "Cat2", Description = "Desc2" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var result = await _repository.GetCategoryByIdAsync(category.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Cat2"));
    }

    [Test]
    public async Task GetAllCategoriesAsync_ReturnsAllCategories()
    {
        var categories = new List<Category>
        {
            new Category { Title = "A", Description = "A" },
            new Category { Title = "B", Description = "B" }
        };
        _context.Categories.AddRange(categories);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllCategoriesAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task UpdateCategoryAsync_UpdatesCategory()
    {
        var category = new Category { Title = "Old", Description = "OldDesc" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        category.Title = "New";
        var updated = await _repository.UpdateCategoryAsync(category);
        await _repository.SaveChangesAsync();

        var result = await _repository.GetCategoryByIdAsync(category.Id);
        Assert.That(result.Title, Is.EqualTo("New"));
    }

    [Test]
    public async Task DeleteCategoryAsync_RemovesCategory()
    {
        var category = new Category { Title = "Del", Description = "DelDesc" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        await _repository.DeleteCategoryAsync(category.Id);
        await _repository.SaveChangesAsync();

        var exists = await _context.Categories.AnyAsync(c => c.Id == category.Id);
        Assert.That(exists, Is.False);
    }
}