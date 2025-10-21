using ElixBackend.Business.DTO;
using ElixBackend.Business.Service;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Moq;

namespace ElixBackend.Tests.Business.Services;

[TestFixture]
public class QuestionServiceTest
{
    private Mock<IQuestionRepository> _questionRepositoryMock;
    private QuestionService _questionService;

    [SetUp]
    public void SetUp()
    {
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _questionService = new QuestionService(_questionRepositoryMock.Object);
    }

    [Test]
    public async Task AddQuestionAsync_CallsRepositoryAndReturnsDto()
    {
        var dto = new QuestionDto { Title = "Q" };
        var question = new Question { Id = 1, Title = "Q" };
        _questionRepositoryMock.Setup(r => r.AddQuestionAsync(It.IsAny<Question>())).ReturnsAsync(question);
        _questionRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _questionService.AddQuestionAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(question.Id));
        Assert.That(result.Title, Is.EqualTo(question.Title));
        _questionRepositoryMock.Verify(r => r.AddQuestionAsync(It.Is<Question>(q => q.Title == dto.Title)), Times.Once);
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
}