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
        var category = new Category { Title = "Cat X" };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var quiz = new Quiz { Title = "New Quiz", CategoryId = category.Id };
        var result = await _repository.AddQuizAsync(quiz);
        await _repository.SaveChangesAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("New Quiz"));
        Assert.That(await _context.Quizzes.AnyAsync(q => q.Title == "New Quiz"), Is.True);
    }

    [Test]
    public async Task GetQuizByIdAsync_ReturnsQuizWithCategory()
    {
        var category = new Category { Title = "Cat 1", Description = "Desc" };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var quiz = new Quiz { Title = "Quiz 1", CategoryId = category.Id };
        await _context.Quizzes.AddAsync(quiz);
        await _context.SaveChangesAsync();

        var result = await _repository.GetQuizByIdAsync(quiz.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Title, Is.EqualTo("Quiz 1"));
        Assert.That(result.Category, Is.Not.Null);
        Assert.That(result.Category!.Title, Is.EqualTo("Cat 1"));
    }

    [Test]
    public async Task GetAllQuizzesAsync_ReturnsAllQuizzes()
    {
        var category = new Category { Title = "Cat A" };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        _context.Quizzes.AddRange(
            new Quiz { Title = "Q1", CategoryId = category.Id },
            new Quiz { Title = "Q2", CategoryId = category.Id }
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllQuizzesAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.Any(q => q.Title == "Q1"), Is.True);
        Assert.That(result.Any(q => q.Title == "Q2"), Is.True);
    }

    [Test]
    public async Task UpdateQuizAsync_UpdatesExistingQuiz()
    {
        var category = new Category { Title = "C" };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var quiz = new Quiz { Title = "Before", CategoryId = category.Id };
        await _context.Quizzes.AddAsync(quiz);
        await _context.SaveChangesAsync();

        quiz.Title = "After";
        var updated = await _repository.UpdateQuizAsync(quiz);
        await _repository.SaveChangesAsync();

        var found = await _repository.GetQuizByIdAsync(quiz.Id);
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Title, Is.EqualTo("After"));
    }

    [Test]
    public async Task DeleteQuizAsync_RemovesQuiz()
    {
        var category = new Category { Title = "D" };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var quiz = new Quiz { Title = "ToDelete", CategoryId = category.Id };
        await _context.Quizzes.AddAsync(quiz);
        await _context.SaveChangesAsync();

        await _repository.DeleteQuizAsync(quiz.Id);
        await _repository.SaveChangesAsync();

        var found = await _repository.GetQuizByIdAsync(quiz.Id);
        Assert.That(found, Is.Null);
    }
}
