using ElixBackend.API.Controllers;
using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ElixBackend.Tests.APi.Controllers;

[TestFixture]
public class QuestionControllerTest
{
    private Mock<IQuestionService> _questionServiceMock;
    private QuestionController _controller;

    [SetUp]
    public void SetUp()
    {
        _questionServiceMock = new Mock<IQuestionService>();
        _controller = new QuestionController(_questionServiceMock.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOkWithQuestions()
    {
        var questions = new List<QuestionDto>
        {
            new QuestionDto { Id = 1, Title = "Q1", CategoryId = 1, MediaPath = null },
            new QuestionDto { Id = 2, Title = "Q2", CategoryId = 1, MediaPath = "path" }
        };
        _questionServiceMock.Setup(s => s.GetAllQuestionsAsync()).ReturnsAsync(questions);

        var result = await _controller.GetAll();

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = result.Result as OkObjectResult;
        Assert.That(ok.Value, Is.EqualTo(questions));
    }

    [Test]
    public async Task GetById_ReturnsOkIfFound()
    {
        var dto = new QuestionDto { Id = 1, Title = "Q1", CategoryId = 1 };
        _questionServiceMock.Setup(s => s.GetQuestionByIdAsync(1)).ReturnsAsync(dto);

        var result = await _controller.GetById(1);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        if (result.Result is OkObjectResult ok) Assert.That(ok.Value, Is.EqualTo(dto));
    }

    [Test]
    public async Task GetById_ReturnsNotFoundIfNull()
    {
        _questionServiceMock.Setup(s => s.GetQuestionByIdAsync(99)).ReturnsAsync((QuestionDto?)null);

        var result = await _controller.GetById(99);

        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var dto = new QuestionDto { Title = "NewQ", CategoryId = 2 };
        var created = new QuestionDto { Id = 3, Title = "NewQ", CategoryId = 2 };
        _questionServiceMock.Setup(s => s.AddQuestionAsync(dto)).ReturnsAsync(created);

        var result = await _controller.Create(dto);

        Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
        var createdResult = result.Result as CreatedAtActionResult;
        if (createdResult != null) Assert.That(createdResult.Value, Is.EqualTo(created));
    }

    [Test]
    public async Task Update_ReturnsOkIfValid()
    {
        var dto = new QuestionDto { Id = 4, Title = "UpQ", CategoryId = 1 };
        var updated = new QuestionDto { Id = 4, Title = "UpQ", CategoryId = 1 };
        _questionServiceMock.Setup(s => s.UpdateQuestionAsync(dto)).ReturnsAsync(updated);

        var result = await _controller.Update(4, dto);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = result.Result as OkObjectResult;
        Assert.That(ok.Value, Is.EqualTo(updated));
    }

    [Test]
    public async Task Update_ReturnsBadRequestIfIdMismatch()
    {
        var dto = new QuestionDto { Id = 5, Title = "X", CategoryId = 1 };

        var result = await _controller.Update(6, dto);

        Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
    }

    [Test]
    public async Task Delete_ReturnsNoContent()
    {
        _questionServiceMock.Setup(s => s.DeleteQuestionAsync(7)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(7);

        Assert.That(result, Is.TypeOf<NoContentResult>());
    }
}