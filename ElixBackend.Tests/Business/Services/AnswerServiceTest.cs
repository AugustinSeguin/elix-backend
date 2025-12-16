using ElixBackend.Business.DTO;
using ElixBackend.Business.Service;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ElixBackend.Tests.Business.Services;

[TestFixture]
public class AnswerServiceTest
{
    private Mock<IAnswerRepository> _answerRepositoryMock;
    private Mock<ILogger<AnswerService>> _loggerMock;
    private AnswerService _service;

    [SetUp]
    public void SetUp()
    {
        _answerRepositoryMock = new Mock<IAnswerRepository>();
        _loggerMock = new Mock<ILogger<AnswerService>>();
        _service = new AnswerService(_answerRepositoryMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task GetByIdAsync_ReturnsDto_WhenFound()
    {
        var answer = new Answer { Id = 1, Title = "A", QuestionId = 2, IsValid = true, Explanation = "exp" };
        _answerRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(answer);
        var result = await _service.GetByIdAsync(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(answer.Id));
        Assert.That(result.Title, Is.EqualTo(answer.Title));
        Assert.That(result.QuestionId, Is.EqualTo(answer.QuestionId));
        Assert.That(result.IsValid, Is.EqualTo(answer.IsValid));
        Assert.That(result.Explanation, Is.EqualTo(answer.Explanation));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _answerRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Answer?)null);
        var result = await _service.GetByIdAsync(99);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_ReturnsDtos()
    {
        var answers = new List<Answer>
        {
            new() { Id = 1, Title = "A", QuestionId = 2, IsValid = true, Explanation = "expA" },
            new() { Id = 2, Title = "B", QuestionId = 3, IsValid = false, Explanation = "expB" }
        };
        _answerRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(answers);
        var result = await _service.GetAllAsync();
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Title, Is.EqualTo("A"));
        Assert.That(result.Last().Title, Is.EqualTo("B"));
    }

    [Test]
    public async Task GetByQuestionIdAsync_ReturnsDtosForQuestion()
    {
        var answers = new List<Answer>
        {
            new() { Id = 10, Title = "Answer1", QuestionId = 5, IsValid = true, Explanation = "exp1" },
            new() { Id = 11, Title = "Answer2", QuestionId = 5, IsValid = false, Explanation = "exp2" }
        };
        _answerRepositoryMock.Setup(r => r.GetByQuestionIdAsync(5)).ReturnsAsync(answers);
        
        var result = await _service.GetByQuestionIdAsync(5);
        
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.All(d => d.QuestionId == 5), Is.True);
        Assert.That(result.Any(d => d.Title == "Answer1"), Is.True);
        Assert.That(result.Any(d => d.Title == "Answer2"), Is.True);
        _answerRepositoryMock.Verify(r => r.GetByQuestionIdAsync(5), Times.Once);
    }

    [Test]
    public async Task GetByQuestionIdAsync_ReturnsEmptyWhenNoQuestionMatch()
    {
        _answerRepositoryMock.Setup(r => r.GetByQuestionIdAsync(999)).ReturnsAsync(new List<Answer>());
        
        var result = await _service.GetByQuestionIdAsync(999);
        
        Assert.That(result.Count(), Is.EqualTo(0));
        _answerRepositoryMock.Verify(r => r.GetByQuestionIdAsync(999), Times.Once);
    }

    [Test]
    public async Task AddAsync_CallsRepositoryWithCorrectEntity()
    {
        var dto = new AnswerDto { Id = 1, Title = "A", QuestionId = 2, IsValid = true, Explanation = "exp" };
        Answer? captured = null;
        _answerRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Answer>())).Callback<Answer>(a => captured = a).Returns(Task.CompletedTask);
        await _service.AddAsync(dto);
        Assert.That(captured, Is.Not.Null);
        Assert.That(captured.Title, Is.EqualTo(dto.Title));
        Assert.That(captured.QuestionId, Is.EqualTo(dto.QuestionId));
        Assert.That(captured.IsValid, Is.EqualTo(dto.IsValid));
        Assert.That(captured.Explanation, Is.EqualTo(dto.Explanation));
    }

    [Test]
    public async Task UpdateAsync_CallsRepositoryWithCorrectEntity()
    {
        var dto = new AnswerDto { Id = 2, Title = "B", QuestionId = 3, IsValid = false, Explanation = "expB" };
        Answer? captured = null;
        _answerRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Answer>())).Callback<Answer>(a => captured = a).Returns(Task.CompletedTask);
        await _service.UpdateAsync(dto);
        Assert.That(captured, Is.Not.Null);
        Assert.That(captured.Id, Is.EqualTo(dto.Id));
        Assert.That(captured.Title, Is.EqualTo(dto.Title));
        Assert.That(captured.QuestionId, Is.EqualTo(dto.QuestionId));
        Assert.That(captured.IsValid, Is.EqualTo(dto.IsValid));
        Assert.That(captured.Explanation, Is.EqualTo(dto.Explanation));
    }

    [Test]
    public async Task DeleteAsync_CallsRepository()
    {
        var called = false;
        _answerRepositoryMock.Setup(r => r.DeleteAsync(5)).Callback(() => called = true).Returns(Task.CompletedTask);
        await _service.DeleteAsync(5);
        Assert.That(called, Is.True);
    }
}