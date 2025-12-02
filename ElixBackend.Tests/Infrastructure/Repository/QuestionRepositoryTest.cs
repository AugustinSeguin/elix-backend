using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure;
using ElixBackend.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Tests.Infrastructure.Repository;

[TestFixture]
public class QuestionRepositoryTest
{
    private ElixDbContext _context;
    private QuestionRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ElixDbContext>()
            .UseInMemoryDatabase(databaseName: TestContext.CurrentContext.Test.Name)
            .Options;
        _context = new ElixDbContext(options);
        _repository = new QuestionRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddQuestionAsync_AddsQuestionAndReturnsIt()
    {
        var question = new Question { Title = "Q1", MediaPath = "p" };

        var result = await _repository.AddQuestionAsync(question);
        await _repository.SaveChangesAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Q1"));
        Assert.That(await _context.Questions.AnyAsync(q => q.Title == "Q1"), Is.True);
    }

    [Test]
    public async Task GetQuestionByIdAsync_ReturnsQuestion()
    {
        var question = new Question { Title = "Q2", MediaPath = "m" };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        var result = await _repository.GetQuestionByIdAsync(question.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Q2"));
    }

    [Test]
    public async Task GetAllQuestionsAsync_ReturnsAllQuestions()
    {
        var questions = new List<Question>
        {
            new Question { Title = "A" },
            new Question { Title = "B" }
        };
        _context.Questions.AddRange(questions);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllQuestionsAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task UpdateQuestionAsync_UpdatesQuestion()
    {
        var question = new Question { Title = "Old", MediaPath = "old" };
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        question.Title = "New";
        await _repository.UpdateQuestionAsync(question);
        await _repository.SaveChangesAsync();

        var result = await _repository.GetQuestionByIdAsync(question.Id);
        Assert.That(result.Title, Is.EqualTo("New"));
    }

    [Test]
    public async Task DeleteQuestionAsync_RemovesQuestion()
    {
        var question = new Question { Id = 1, Title = "Q" };
        await _repository.AddQuestionAsync(question);
        await _repository.SaveChangesAsync();

        await _repository.DeleteQuestionAsync(1);
        await _repository.SaveChangesAsync();

        var found = await _repository.GetQuestionByIdAsync(1);
        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task GetQuestionsByCategoryIdAsync_ReturnsQuestionsForCategory()
    {
        var questions = new List<Question>
        {
            new Question { Title = "Q1", CategoryId = 1 },
            new Question { Title = "Q2", CategoryId = 1 },
            new Question { Title = "Q3", CategoryId = 2 }
        };
        foreach (var q in questions)
        {
            await _repository.AddQuestionAsync(q);
        }
        await _repository.SaveChangesAsync();

        var result = await _repository.GetQuestionsByCategoryIdAsync(1);
        var resultList = result.ToList();

        Assert.That(resultList.Count, Is.EqualTo(2));
        Assert.That(resultList.All(q => q.CategoryId == 1), Is.True);
    }

    [Test]
    public async Task GetQuestionsByCategoryIdAsync_ReturnsEmptyWhenNoCategoryMatch()
    {
        var question = new Question { Title = "Q1", CategoryId = 1 };
        await _repository.AddQuestionAsync(question);
        await _repository.SaveChangesAsync();

        var result = await _repository.GetQuestionsByCategoryIdAsync(99);

        Assert.That(result.Count(), Is.EqualTo(0));
    }
}