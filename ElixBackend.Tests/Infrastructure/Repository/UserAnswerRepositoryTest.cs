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
    public async Task AddUserAnswerAsync_AddsUserAnswerAndReturnsIt()
    {
        var userAnswer = new UserAnswer { UserId = 1, QuestionId = 1, IsCorrect = true };

        var result = await _repository.AddUserAnswerAsync(userAnswer);
        await _repository.SaveChangesAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(1));
        Assert.That(result.QuestionId, Is.EqualTo(1));
        Assert.That(result.IsCorrect, Is.True);
        Assert.That(await _context.UserAnswers.AnyAsync(ua => ua.UserId == 1 && ua.QuestionId == 1), Is.True);
    }

    [Test]
    public async Task GetUserAnswerByUserIdAsync_ReturnsUserAnswers()
    {
        var userAnswer1 = new UserAnswer { UserId = 2, QuestionId = 3, IsCorrect = true };
        var userAnswer2 = new UserAnswer { UserId = 2, QuestionId = 3, IsCorrect = false };
        var userAnswer3 = new UserAnswer { UserId = 2, QuestionId = 4, IsCorrect = true };
        _context.UserAnswers.AddRange(userAnswer1, userAnswer2, userAnswer3);
        await _context.SaveChangesAsync();

        var result = await _repository.GetUserAnswerByUserIdAsync(2, 3);
        var resultList = result.ToList();

        Assert.That(resultList.Count, Is.EqualTo(2));
        Assert.That(resultList.All(ua => ua!.UserId == 2 && ua.QuestionId == 3), Is.True);
    }

    [Test]
    public async Task GetUserAnswerByUserIdAsync_ReturnsEmptyWhenNoMatch()
    {
        var userAnswer = new UserAnswer { UserId = 1, QuestionId = 1, IsCorrect = true };
        _context.UserAnswers.Add(userAnswer);
        await _context.SaveChangesAsync();

        var result = await _repository.GetUserAnswerByUserIdAsync(99, 99);

        Assert.That(result.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task UpdateUserAnswerAsync_UpdatesUserAnswer()
    {
        var userAnswer = new UserAnswer { UserId = 3, QuestionId = 5, IsCorrect = false };
        _context.UserAnswers.Add(userAnswer);
        await _context.SaveChangesAsync();

        userAnswer.IsCorrect = true;
        await _repository.UpdateUserAnswerAsync(userAnswer);
        await _repository.SaveChangesAsync();

        var result = await _context.UserAnswers.FindAsync(userAnswer.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsCorrect, Is.True);
    }

    [Test]
    public async Task DeleteUserAnswerAsync_RemovesUserAnswer()
    {
        var userAnswer = new UserAnswer { UserId = 4, QuestionId = 6, IsCorrect = true };
        await _repository.AddUserAnswerAsync(userAnswer);
        await _repository.SaveChangesAsync();
        var id = userAnswer.Id;

        await _repository.DeleteUserAnswerAsync(id);
        await _repository.SaveChangesAsync();

        var found = await _context.UserAnswers.FindAsync(id);
        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task DeleteUserAnswerAsync_DoesNothingWhenIdNotFound()
    {
        await _repository.DeleteUserAnswerAsync(999);
        await _repository.SaveChangesAsync();

        // Should not throw
        Assert.Pass();
    }

    [Test]
    public async Task SaveChangesAsync_ReturnsTrueWhenChangesAreSaved()
    {
        var userAnswer = new UserAnswer { UserId = 5, QuestionId = 7, IsCorrect = false };
        await _repository.AddUserAnswerAsync(userAnswer);

        var result = await _repository.SaveChangesAsync();

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task SaveChangesAsync_ReturnsFalseWhenNoChanges()
    {
        var result = await _repository.SaveChangesAsync();

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task AddUserAnswerAsync_WithDefaultIsCorrect_ShouldBeFalse()
    {
        var userAnswer = new UserAnswer { UserId = 6, QuestionId = 8 };

        var result = await _repository.AddUserAnswerAsync(userAnswer);
        await _repository.SaveChangesAsync();

        Assert.That(result.IsCorrect, Is.False);
    }

    [Test]
    public async Task GetUserAnswerByUserIdAsync_FiltersByUserIdAndQuestionId()
    {
        var userAnswer1 = new UserAnswer { UserId = 7, QuestionId = 9, IsCorrect = true };
        var userAnswer2 = new UserAnswer { UserId = 7, QuestionId = 10, IsCorrect = false };
        var userAnswer3 = new UserAnswer { UserId = 8, QuestionId = 9, IsCorrect = true };
        _context.UserAnswers.AddRange(userAnswer1, userAnswer2, userAnswer3);
        await _context.SaveChangesAsync();

        var result = await _repository.GetUserAnswerByUserIdAsync(7, 9);
        var resultList = result.ToList();

        Assert.That(resultList.Count, Is.EqualTo(1));
        Assert.That(resultList[0]!.UserId, Is.EqualTo(7));
        Assert.That(resultList[0]!.QuestionId, Is.EqualTo(9));
    }
}