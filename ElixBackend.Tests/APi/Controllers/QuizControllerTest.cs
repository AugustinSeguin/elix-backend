using ElixBackend.API.Controllers;
using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ElixBackend.Tests.APi.Controllers;

[TestFixture]
public class QuizControllerTest
{
    private Mock<IQuizService> _quizServiceMock;
    private QuizController _controller;

    [SetUp]
    public void SetUp()
    {
        _quizServiceMock = new Mock<IQuizService>();
        _controller = new QuizController(_quizServiceMock.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOkWithQuizzes()
    {
        var quizzes = new List<QuizDto>
        {
            new QuizDto { Id = 1, Title = "Quiz1", CategoryId = 1 },
            new QuizDto { Id = 2, Title = "Quiz2", CategoryId = 2 }
        };
        _quizServiceMock.Setup(s => s.GetAllQuizzesAsync()).ReturnsAsync(quizzes);

        var result = await _controller.GetAll();

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = result.Result as OkObjectResult;
        Assert.That(ok.Value, Is.EqualTo(quizzes));
    }

    [Test]
    public async Task GetById_ReturnsOkIfFound()
    {
        var dto = new QuizDto { Id = 1, Title = "Quiz1", CategoryId = 1 };
        _quizServiceMock.Setup(s => s.GetQuizByIdAsync(1)).ReturnsAsync(dto);

        var result = await _controller.GetById(1);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        if (result.Result is OkObjectResult ok) Assert.That(ok.Value, Is.EqualTo(dto));
    }

    [Test]
    public async Task GetById_ReturnsNotFoundIfNull()
    {
        _quizServiceMock.Setup(s => s.GetQuizByIdAsync(99)).ReturnsAsync((QuizDto?)null);

        var result = await _controller.GetById(99);

        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var dto = new QuizDto { Title = "NewQuiz", CategoryId = 1 };
        var created = new QuizDto { Id = 3, Title = "NewQuiz", CategoryId = 1 };
        _quizServiceMock.Setup(s => s.AddQuizAsync(dto)).ReturnsAsync(created);

        var result = await _controller.Create(dto);

        Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
        var createdResult = result.Result as CreatedAtActionResult;
        if (createdResult != null) Assert.That(createdResult.Value, Is.EqualTo(created));
    }

    [Test]
    public async Task Update_ReturnsOkIfValid()
    {
        var dto = new QuizDto { Id = 4, Title = "UpQuiz", CategoryId = 2 };
        var updated = new QuizDto { Id = 4, Title = "UpQuiz", CategoryId = 2 };
        _quizServiceMock.Setup(s => s.UpdateQuizAsync(dto)).ReturnsAsync(updated);

        var result = await _controller.Update(4, dto);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = result.Result as OkObjectResult;
        Assert.That(ok.Value, Is.EqualTo(updated));
    }

    [Test]
    public async Task Update_ReturnsBadRequestIfIdMismatch()
    {
        var dto = new QuizDto { Id = 5, Title = "X", CategoryId = 1 };

        var result = await _controller.Update(6, dto);

        Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
    }

    [Test]
    public async Task Delete_ReturnsNoContent()
    {
        _quizServiceMock.Setup(s => s.DeleteQuizAsync(7)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(7);

        Assert.That(result, Is.TypeOf<NoContentResult>());
    }
}