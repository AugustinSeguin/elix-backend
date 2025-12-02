using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure;
using ElixBackend.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Tests.Infrastructure.Repository;

[TestFixture]
public class UserAnswerRepositoryTest
{
    private ElixDbContext _context;
    private UserAnswerRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ElixDbContext>()
            .UseInMemoryDatabase(databaseName: TestContext.CurrentContext.Test.Name)
            .Options;
        _context = new ElixDbContext(options);
        _repository = new UserAnswerRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddUserAnswerAsync_AddsAndReturnsEntity()
    {
        var ua = new UserAnswer { UserId = 1, AnswerId = 2, IsCorrect = true };

        var result = await _repository.AddUserAnswerAsync(ua);
        await _repository.SaveChangesAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsCorrect, Is.True);
        Assert.That(await _context.Set<UserAnswer>().AnyAsync(x => x.UserId == 1 && x.AnswerId == 2), Is.True);
    }

    [Test]
    public async Task GetUserAnswerByIdAsync_ReturnsEntity()
    {
        var ua = new UserAnswer { UserId = 3, AnswerId = 4, IsCorrect = false };
        await _repository.AddUserAnswerAsync(ua);
        await _repository.SaveChangesAsync();

        var result = await _repository.GetUserAnswerByIdAsync(ua.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(3));
    }

    [Test]
    public async Task GetAllUserAnswersAsync_ReturnsAll()
    {
        var list = new List<UserAnswer>
        {
            new UserAnswer { UserId = 10, AnswerId = 11, IsCorrect = false },
            new UserAnswer { UserId = 12, AnswerId = 13, IsCorrect = true }
        };

        foreach (var ua in list)
        {
            await _repository.AddUserAnswerAsync(ua);
        }
        await _repository.SaveChangesAsync();

        var result = await _repository.GetAllUserAnswersAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task UpdateUserAnswerAsync_UpdatesEntity()
    {
        var ua = new UserAnswer { UserId = 20, AnswerId = 21, IsCorrect = false };
        await _repository.AddUserAnswerAsync(ua);
        await _repository.SaveChangesAsync();

        ua.IsCorrect = true;
        await _repository.UpdateUserAnswerAsync(ua);
        await _repository.SaveChangesAsync();

        var fromDb = await _repository.GetUserAnswerByIdAsync(ua.Id);
        Assert.That(fromDb, Is.Not.Null);
        Assert.That(fromDb.IsCorrect, Is.True);
    }

    [Test]
    public async Task DeleteUserAnswerAsync_RemovesEntity()
    {
        var ua = new UserAnswer { UserId = 30, AnswerId = 31, IsCorrect = false };
        await _repository.AddUserAnswerAsync(ua);
        await _repository.SaveChangesAsync();

        await _repository.DeleteUserAnswerAsync(ua.Id);
        await _repository.SaveChangesAsync();

        var found = await _repository.GetUserAnswerByIdAsync(ua.Id);
        Assert.That(found, Is.Null);
    }
}