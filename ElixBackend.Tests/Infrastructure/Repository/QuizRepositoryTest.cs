using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure;
using ElixBackend.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Tests.Infrastructure.Repository;

[TestFixture]
public class QuizRepositoryTest
{
    private ElixDbContext _context;
    private QuizRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ElixDbContext>()
            .UseInMemoryDatabase(databaseName: TestContext.CurrentContext.Test.Name)
            .Options;
        _context = new ElixDbContext(options);
        _repository = new QuizRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddQuizAsync_AddsQuizAndReturnsIt()
    {
        var quiz = new Quiz { Title = "Quiz1", CategoryId = 1 };

        var result = await _repository.AddQuizAsync(quiz);
        await _repository.SaveChangesAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Quiz1"));
        Assert.That(await _context.Quizzes.AnyAsync(q => q.Title == "Quiz1"), Is.True);
    }

    [Test]
    public async Task GetQuizByIdAsync_ReturnsQuiz()
    {
        var quiz = new Quiz { Title = "Quiz2", CategoryId = 2 };
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        var result = await _repository.GetQuizByIdAsync(quiz.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Quiz2"));
        Assert.That(result.CategoryId, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllQuizzesAsync_ReturnsAllQuizzes()
    {
        var quizzes = new List<Quiz>
        {
            new Quiz { Title = "A", CategoryId = 1 },
            new Quiz { Title = "B", CategoryId = 2 }
        };
        _context.Quizzes.AddRange(quizzes);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllQuizzesAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.Any(q => q.Title == "A"));
        Assert.That(result.Any(q => q.Title == "B"));
    }

    [Test]
    public async Task UpdateQuizAsync_UpdatesQuiz()
    {
        var quiz = new Quiz { Title = "Old", CategoryId = 1 };
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        quiz.Title = "New";
        var updated = await _repository.UpdateQuizAsync(quiz);
        await _repository.SaveChangesAsync();

        var result = await _repository.GetQuizByIdAsync(quiz.Id);
        Assert.That(result.Title, Is.EqualTo("New"));
    }

    [Test]
    public async Task DeleteQuizAsync_RemovesQuiz()
    {
        var quiz = new Quiz { Id = 1, Title = "Quiz", CategoryId = 1 };
        await _repository.AddQuizAsync(quiz);
        await _repository.SaveChangesAsync();

        await _repository.DeleteQuizAsync(1);
        await _repository.SaveChangesAsync();

        var found = await _repository.GetQuizByIdAsync(1);
        Assert.That(found, Is.Null);
    }
}