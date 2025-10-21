using ElixBackend.API.Controllers;
using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace ElixBackend.Tests.APi.Controllers;

[TestFixture]
public class AnswerControllerTest
{
    private Mock<IAnswerService> _answerServiceMock;
    private AnswerController _controller;

    [SetUp]
    public void SetUp()
    {
        _answerServiceMock = new Mock<IAnswerService>();
        _controller = new AnswerController(_answerServiceMock.Object);
    }

    [Test]
    public async Task GetAll_ReturnsOkWithAnswers()
    {
        var answers = new List<AnswerDto> { new() { Id = 1, Title = "A", QuestionId = 1, IsValid = true } };
        _answerServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(answers);
        var result = await _controller.GetAll();
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var ok = result.Result as OkObjectResult;
        Assert.That(ok.Value, Is.EqualTo(answers));
    }

    [Test]
    public async Task GetById_ReturnsOkIfFound()
    {
        var answer = new AnswerDto { Id = 1, Title = "A", QuestionId = 1, IsValid = true };
        _answerServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(answer);
        var result = await _controller.GetById(1);
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        if (result.Result is OkObjectResult ok) Assert.That(ok.Value, Is.Not.Null);
    }

    [Test]
    public async Task GetById_ReturnsNotFoundIfNull()
    {
        _answerServiceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((AnswerDto?)null);
        var result = await _controller.GetById(99);
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Add_ReturnsCreatedAtAction()
    {
        var dto = new AnswerDto { Id = 1, Title = "A", QuestionId = 1, IsValid = true };
        _answerServiceMock.Setup(s => s.AddAsync(dto)).Returns(Task.CompletedTask);
        var result = await _controller.Add(dto);
        Assert.That(result, Is.TypeOf<CreatedAtActionResult>());
        var created = result as CreatedAtActionResult;
        Assert.That(created.Value, Is.EqualTo(dto));
    }

    [Test]
    public async Task Update_ReturnsNoContent_WhenIdMatches()
    {
        var dto = new AnswerDto { Id = 1, Title = "A", QuestionId = 1, IsValid = true };
        _answerServiceMock.Setup(s => s.UpdateAsync(dto)).Returns(Task.CompletedTask);
        var result = await _controller.Update(1, dto);
        Assert.That(result, Is.TypeOf<NoContentResult>());
    }

    [Test]
    public async Task Update_ReturnsBadRequest_WhenIdMismatch()
    {
        var dto = new AnswerDto { Id = 2, Title = "A", QuestionId = 1, IsValid = true };
        var result = await _controller.Update(1, dto);
        Assert.That(result, Is.TypeOf<BadRequestResult>());
    }

    [Test]
    public async Task Delete_ReturnsNoContent()
    {
        _answerServiceMock.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);
        var result = await _controller.Delete(1);
        Assert.That(result, Is.TypeOf<NoContentResult>());
    }
}