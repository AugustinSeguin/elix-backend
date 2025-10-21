using ElixBackend.Business.DTO;
using ElixBackend.Business.Service;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Moq;

namespace ElixBackend.Tests.Business.Services;

[TestFixture]
public class QuizServiceTest
{
    private Mock<IQuizRepository> _quizRepositoryMock;
    private QuizService _quizService;

    [SetUp]
    public void SetUp()
    {
        _quizRepositoryMock = new Mock<IQuizRepository>();
        _quizService = new QuizService(_quizRepositoryMock.Object);
    }

    [Test]
    public async Task AddQuizAsync_CallsRepositoryAndReturnsDto()
    {
        var dto = new QuizDto { Title = "Quiz", CategoryId = 1 };
        var quiz = new Quiz { Id = 1, Title = "Quiz", CategoryId = 1 };
        _quizRepositoryMock.Setup(r => r.AddQuizAsync(It.IsAny<Quiz>())).ReturnsAsync(quiz);
        _quizRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _quizService.AddQuizAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(quiz.Id));
        Assert.That(result.Title, Is.EqualTo(quiz.Title));
        Assert.That(result.CategoryId, Is.EqualTo(quiz.CategoryId));
        _quizRepositoryMock.Verify(r => r.AddQuizAsync(It.Is<Quiz>(q => q.Title == dto.Title && q.CategoryId == dto.CategoryId)), Times.Once);
        _quizRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetQuizByIdAsync_ReturnsDto()
    {
        var quiz = new Quiz { Id = 2, Title = "Q2", CategoryId = 1 };
        _quizRepositoryMock.Setup(r => r.GetQuizByIdAsync(2)).ReturnsAsync(quiz);

        var result = await _quizService.GetQuizByIdAsync(2);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(quiz.Id));
        Assert.That(result.Title, Is.EqualTo(quiz.Title));
        Assert.That(result.CategoryId, Is.EqualTo(quiz.CategoryId));
    }

    [Test]
    public async Task GetAllQuizzesAsync_ReturnsDtos()
    {
        var quizzes = new List<Quiz>
        {
            new Quiz { Id = 1, Title = "A", CategoryId = 1 },
            new Quiz { Id = 2, Title = "B", CategoryId = 2 }
        };
        _quizRepositoryMock.Setup(r => r.GetAllQuizzesAsync()).ReturnsAsync(quizzes);

        var result = await _quizService.GetAllQuizzesAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.Any(d => d.Title == "A"));
        Assert.That(result.Any(d => d.Title == "B"));
    }

    [Test]
    public async Task UpdateQuizAsync_CallsRepositoryAndReturnsDto()
    {
        var dto = new QuizDto { Id = 3, Title = "Up", CategoryId = 2 };
        var quiz = new Quiz { Id = 3, Title = "Up", CategoryId = 2 };
        _quizRepositoryMock.Setup(r => r.UpdateQuizAsync(It.IsAny<Quiz>())).ReturnsAsync(quiz);
        _quizRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _quizService.UpdateQuizAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(quiz.Id));
        Assert.That(result.Title, Is.EqualTo(quiz.Title));
        Assert.That(result.CategoryId, Is.EqualTo(quiz.CategoryId));
        _quizRepositoryMock.Verify(r => r.UpdateQuizAsync(It.Is<Quiz>(q => q.Id == dto.Id && q.Title == dto.Title && q.CategoryId == dto.CategoryId)), Times.Once);
        _quizRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteQuizAsync_CallsRepository()
    {
        _quizRepositoryMock.Setup(r => r.DeleteQuizAsync(4)).Returns(Task.CompletedTask);
        _quizRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        await _quizService.DeleteQuizAsync(4);

        _quizRepositoryMock.Verify(r => r.DeleteQuizAsync(4), Times.Once);
        _quizRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task SaveChangesAsync_CallsRepositoryAndReturnsResult()
    {
        _quizRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _quizService.SaveChangesAsync();

        Assert.That(result, Is.True);
        _quizRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}