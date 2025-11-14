using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using ElixBackend.Infrastructure;

namespace ElixBackend.Tests.Infrastructure.Repository;

[TestFixture]
public class AnswerRepositoryTest
{
    private ElixDbContext _context;
    private AnswerRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ElixDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ElixDbContext(options);
        _repository = new AnswerRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddAsync_AddsAnswer()
    {
        var answer = new Answer { Id = 1, Title = "Réponse A", QuestionId = 1, IsValid = true };
        await _repository.AddAsync(answer);
        var found = await _repository.GetByIdAsync(1);
        Assert.That(found, Is.Not.Null);
        Assert.That(found.Title, Is.EqualTo("Réponse A"));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsAnswer_WhenExists()
    {
        var answer = new Answer { Id = 2, Title = "Réponse B", QuestionId = 2, IsValid = false };
        await _repository.AddAsync(answer);
        var found = await _repository.GetByIdAsync(2);
        Assert.That(found, Is.Not.Null);
        Assert.That(found.Title, Is.EqualTo("Réponse B"));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        var found = await _repository.GetByIdAsync(999);
        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllAnswers()
    {
        await _repository.AddAsync(new Answer { Id = 3, Title = "A", QuestionId = 1, IsValid = true });
        await _repository.AddAsync(new Answer { Id = 4, Title = "B", QuestionId = 2, IsValid = false });
        var all = await _repository.GetAllAsync();
        Assert.That(all.Count(), Is.EqualTo(2));
        Assert.That(all.Any(a => a.Title == "A"), Is.True);
        Assert.That(all.Any(a => a.Title == "B"), Is.True);
    }

    [Test]
    public async Task GetByQuestionIdAsync_ReturnsAnswersForQuestion()
    {
        await _repository.AddAsync(new Answer { Id = 10, Title = "Answer1", QuestionId = 5, IsValid = true });
        await _repository.AddAsync(new Answer { Id = 11, Title = "Answer2", QuestionId = 5, IsValid = false });
        await _repository.AddAsync(new Answer { Id = 12, Title = "Answer3", QuestionId = 6, IsValid = true });
        
        var result = await _repository.GetByQuestionIdAsync(5);
        
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.All(a => a.QuestionId == 5), Is.True);
        Assert.That(result.Any(a => a.Title == "Answer1"), Is.True);
        Assert.That(result.Any(a => a.Title == "Answer2"), Is.True);
    }

    [Test]
    public async Task GetByQuestionIdAsync_ReturnsEmptyWhenNoQuestionMatch()
    {
        await _repository.AddAsync(new Answer { Id = 20, Title = "Answer1", QuestionId = 1, IsValid = true });
        await _repository.AddAsync(new Answer { Id = 21, Title = "Answer2", QuestionId = 2, IsValid = false });
        
        var result = await _repository.GetByQuestionIdAsync(999);
        
        Assert.That(result.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task UpdateAsync_UpdatesAnswer()
    {
        var answer = new Answer { Id = 5, Title = "Ancien", QuestionId = 1, IsValid = false };
        await _repository.AddAsync(answer);
        answer.Title = "Nouveau";
        answer.IsValid = true;
        await _repository.UpdateAsync(answer);
        var updated = await _repository.GetByIdAsync(5);
        Assert.That(updated.Title, Is.EqualTo("Nouveau"));
        Assert.That(updated.IsValid, Is.True);
    }

    [Test]
    public async Task DeleteAsync_RemovesAnswer()
    {
        var answer = new Answer { Id = 6, Title = "À supprimer", QuestionId = 1, IsValid = false };
        await _repository.AddAsync(answer);
        await _repository.DeleteAsync(6);
        var found = await _repository.GetByIdAsync(6);
        Assert.That(found, Is.Null);
    }
}