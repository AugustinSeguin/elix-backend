using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure;
using ElixBackend.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Tests.Infrastructure.Repository;

[TestFixture]
public class UserPointRepositoryTest
{
    private ElixDbContext _context;
    private UserPointRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ElixDbContext>()
            .UseInMemoryDatabase(databaseName: TestContext.CurrentContext.Test.Name)
            .Options;
        _context = new ElixDbContext(options);
        _repository = new UserPointRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddUserPointAsync_AddsAndReturnsEntity()
    {
        var up = new UserPoint { UserId = 1, CategoryId = 2, Points = 10 };

        var result = await _repository.AddUserPointAsync(up);
        await _repository.SaveChangesAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(1));
        Assert.That(result.CategoryId, Is.EqualTo(2));
        Assert.That(result.Points, Is.EqualTo(10));
        Assert.That(await _context.UserPoints.AnyAsync(x => x.UserId == 1 && x.CategoryId == 2 && x.Points == 10), Is.True);
    }

    [Test]
    public async Task GetUserPointByIdAsync_ReturnsEntity()
    {
        var up = new UserPoint { UserId = 3, CategoryId = 4, Points = 5 };
        await _repository.AddUserPointAsync(up);
        await _repository.SaveChangesAsync();

        var found = await _repository.GetUserPointByIdAsync(up.Id);

        Assert.That(found, Is.Not.Null);
        Assert.That(found.UserId, Is.EqualTo(3));
        Assert.That(found.CategoryId, Is.EqualTo(4));
        Assert.That(found.Points, Is.EqualTo(5));
    }

    [Test]
    public async Task GetAllUserPointsAsync_ReturnsAll()
    {
        var items = new List<UserPoint>
        {
            new UserPoint { UserId = 10, CategoryId = 11, Points = 1 },
            new UserPoint { UserId = 12, CategoryId = 13, Points = 2 }
        };
        foreach (var up in items)
        {
            await _repository.AddUserPointAsync(up);
        }
        await _repository.SaveChangesAsync();

        var all = await _repository.GetAllUserPointsAsync();

        Assert.That(all.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task UpdateUserPointAsync_UpdatesEntity()
    {
        var up = new UserPoint { UserId = 20, CategoryId = 21, Points = 3 };
        await _repository.AddUserPointAsync(up);
        await _repository.SaveChangesAsync();

        up.Points = 8;
        var updated = await _repository.UpdateUserPointAsync(up);
        await _repository.SaveChangesAsync();

        var found = await _repository.GetUserPointByIdAsync(up.Id);
        Assert.That(found, Is.Not.Null);
        Assert.That(found.Points, Is.EqualTo(8));
    }

    [Test]
    public async Task DeleteUserPointAsync_RemovesEntity()
    {
        var up = new UserPoint { UserId = 30, CategoryId = 31, Points = 15 };
        await _repository.AddUserPointAsync(up);
        await _repository.SaveChangesAsync();

        await _repository.DeleteUserPointAsync(up.Id);
        await _repository.SaveChangesAsync();

        var found = await _repository.GetUserPointByIdAsync(up.Id);
        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task GetUserPointsByCategory_ReturnsEntity()
    {
        var up = new UserPoint { UserId = 1, CategoryId = 2, Points = 10 };
        await _repository.AddUserPointAsync(up);
        await _repository.SaveChangesAsync();

        var result = await _repository.GetUserPointsByCategory(2, 1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(1));
        Assert.That(result.CategoryId, Is.EqualTo(2));
        Assert.That(result.Points, Is.EqualTo(10));
    }

    [Test]
    public async Task GetUserPointsByCategory_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetUserPointsByCategory(99, 99);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUserPoints_ReturnsEntities()
    {
        var up1 = new UserPoint { UserId = 1, CategoryId = 2, Points = 10 };
        var up2 = new UserPoint { UserId = 1, CategoryId = 3, Points = 20 };
        var up3 = new UserPoint { UserId = 2, CategoryId = 2, Points = 30 };
        await _repository.AddUserPointAsync(up1);
        await _repository.AddUserPointAsync(up2);
        await _repository.AddUserPointAsync(up3);
        await _repository.SaveChangesAsync();

        var result = await _repository.GetUserPoints(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.Any(x => x.CategoryId == 2 && x.Points == 10), Is.True);
        Assert.That(result.Any(x => x.CategoryId == 3 && x.Points == 20), Is.True);
    }
}