using ElixBackend.Business.DTO;
using ElixBackend.Business.Service;
using ElixBackend.Domain.Entities;
using ElixBackend.Domain.Enum;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.Extensions.Logging;
using Moq;

namespace ElixBackend.Tests.Business.Services;

[TestFixture]
public class QuestionServiceTest
{
    private Mock<IQuestionRepository> _questionRepositoryMock;
    private QuestionService _questionService;
    private Mock<ILogger<QuestionService>> _loggerMock;

    [SetUp]
    public void SetUp()
    {
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _loggerMock = new Mock<ILogger<QuestionService>>();
        _questionService = new QuestionService(_questionRepositoryMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task AddQuestionAsync_CallsRepositoryAndReturnsDto()
    {
        var dto = new QuestionDto { Title = "Q", TypeQuestion = TypeQuestion.TrueFalseActive };
        var question = new Question { Id = 1, Title = "Q", TypeQuestion = TypeQuestion.TrueFalseActive };
        _questionRepositoryMock.Setup(r => r.AddQuestionAsync(It.IsAny<Question>())).ReturnsAsync(question);
        _questionRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _questionService.AddQuestionAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(question.Id));
        Assert.That(result.Title, Is.EqualTo(question.Title));
        Assert.That(result.TypeQuestion, Is.EqualTo(TypeQuestion.TrueFalseActive));
        _questionRepositoryMock.Verify(r => r.AddQuestionAsync(It.Is<Question>(q => q.Title == dto.Title && q.TypeQuestion == TypeQuestion.TrueFalseActive)), Times.Once);
        _questionRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetQuestionByIdAsync_ReturnsDto()
    {
        var question = new Question { Id = 2, Title = "Q2", MediaPath = "p" };
        _questionRepositoryMock.Setup(r => r.GetQuestionByIdAsync(2)).ReturnsAsync(question);

        var result = await _questionService.GetQuestionByIdAsync(2);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(question.Id));
    }

    [Test]
    public async Task GetAllQuestionsAsync_ReturnsDtos()
    {
        var questions = new List<Question>
        {
            new Question { Id = 1, Title = "A" },
            new Question { Id = 2, Title = "B" }
        };
        _questionRepositoryMock.Setup(r => r.GetAllQuestionsAsync()).ReturnsAsync(questions);

        var result = await _questionService.GetAllQuestionsAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.Any(d => d.Title == "A"));
    }

    [Test]
    public async Task UpdateQuestionAsync_CallsRepositoryAndReturnsDto()
    {
        var dto = new QuestionDto { Id = 3, Title = "Up" };
        var question = new Question { Id = 3, Title = "Up" };
        _questionRepositoryMock.Setup(r => r.UpdateQuestionAsync(It.IsAny<Question>())).ReturnsAsync(question);
        _questionRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _questionService.UpdateQuestionAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(question.Id));
        _questionRepositoryMock.Verify(r => r.UpdateQuestionAsync(It.Is<Question>(q => q.Id == dto.Id && q.Title == dto.Title)), Times.Once);
        _questionRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteQuestionAsync_CallsRepository()
    {
        _questionRepositoryMock.Setup(r => r.DeleteQuestionAsync(4)).Returns(Task.CompletedTask);
        _questionRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        await _questionService.DeleteQuestionAsync(4);

        _questionRepositoryMock.Verify(r => r.DeleteQuestionAsync(4), Times.Once);
        _questionRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task SaveChangesAsync_CallsRepositoryAndReturnsResult()
    {
        _questionRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _questionService.SaveChangesAsync();

        Assert.That(result, Is.True);
        _questionRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetQuestionsByCategoryIdAsync_ReturnsDtosForCategory()
    {
        var questions = new List<Question>
        {
            new Question { Id = 1, Title = "Q1", CategoryId = 5 },
            new Question { Id = 2, Title = "Q2", CategoryId = 5 }
        };
        _questionRepositoryMock.Setup(r => r.GetQuestionsByCategoryIdAsync(5)).ReturnsAsync(questions);

        var result = await _questionService.GetQuestionsByCategoryIdAsync(5);
        var resultList = result.ToList();

        Assert.That(resultList.Count, Is.EqualTo(2));
        Assert.That(resultList.All(q => q.CategoryId == 5), Is.True);
        _questionRepositoryMock.Verify(r => r.GetQuestionsByCategoryIdAsync(5), Times.Once);
    }

    [Test]
    public async Task GetQuestionsByCategoryIdAsync_ReturnsEmptyWhenNoMatch()
    {
        _questionRepositoryMock.Setup(r => r.GetQuestionsByCategoryIdAsync(99)).ReturnsAsync(new List<Question>());

        var result = await _questionService.GetQuestionsByCategoryIdAsync(99);

        Assert.That(result.Count(), Is.EqualTo(0));
        _questionRepositoryMock.Verify(r => r.GetQuestionsByCategoryIdAsync(99), Times.Once);
    }
}