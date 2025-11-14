using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.WebApp.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;

namespace ElixBackend.Tests.WebApp.Controllers;

[TestFixture]
public class AnswerControllerTest
{
    private Mock<IAnswerService> _answerServiceMock;
    private Mock<IQuestionService> _questionServiceMock;
    private AnswerController _controller;

    [SetUp]
    public void SetUp()
    {
        _answerServiceMock = new Mock<IAnswerService>();
        _questionServiceMock = new Mock<IQuestionService>();
        _controller = new AnswerController(_answerServiceMock.Object, _questionServiceMock.Object);
        
        // Setup TempData and HttpContext
        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    #region Index Tests

    [Test]
    public async Task Index_WithQuestionId_ReturnsViewWithAnswers()
    {
        // Arrange
        var questionId = 5;
        var answers = new List<AnswerDto>
        {
            new() { Id = 1, QuestionId = questionId, Title = "Answer 1", IsValid = true },
            new() { Id = 2, QuestionId = questionId, Title = "Answer 2", IsValid = false }
        };
        _answerServiceMock.Setup(s => s.GetByQuestionIdAsync(questionId)).ReturnsAsync(answers);

        // Act
        var result = await _controller.Index(questionId) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<IEnumerable<AnswerDto>>());
        var model = result.Model as IEnumerable<AnswerDto>;
        Assert.That(model.Count(), Is.EqualTo(2));
        Assert.That(_controller.ViewBag.QuestionId, Is.EqualTo(questionId));
        _answerServiceMock.Verify(s => s.GetByQuestionIdAsync(questionId), Times.Once);
    }

    [Test]
    public async Task Index_WithoutQuestionId_ReturnsViewWithEmptyList()
    {
        // Act
        var result = await _controller.Index(null) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<IEnumerable<AnswerDto>>());
        var model = result.Model as IEnumerable<AnswerDto>;
        Assert.That(model.Count(), Is.EqualTo(0));
        _answerServiceMock.Verify(s => s.GetByQuestionIdAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region GetFormModal Tests

    [Test]
    public async Task GetFormModal_WithValidId_ReturnsPartialViewWithAnswer()
    {
        // Arrange
        var answerId = 10;
        var answer = new AnswerDto { Id = answerId, QuestionId = 5, Title = "Answer", IsValid = true };
        _answerServiceMock.Setup(s => s.GetByIdAsync(answerId)).ReturnsAsync(answer);

        // Act
        var result = await _controller.GetFormModal(answerId, null) as PartialViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewName, Is.EqualTo("~/Views/Question/Answer/Form.cshtml"));
        Assert.That(result.Model, Is.InstanceOf<AnswerDto>());
        var model = result.Model as AnswerDto;
        Assert.That(model.Id, Is.EqualTo(answerId));
        Assert.That(model.Title, Is.EqualTo("Answer"));
        _answerServiceMock.Verify(s => s.GetByIdAsync(answerId), Times.Once);
    }

    [Test]
    public async Task GetFormModal_WithQuestionId_ReturnsPartialViewWithNewAnswer()
    {
        // Arrange
        var questionId = 7;

        // Act
        var result = await _controller.GetFormModal(null, questionId) as PartialViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewName, Is.EqualTo("~/Views/Question/Answer/Form.cshtml"));
        Assert.That(result.Model, Is.InstanceOf<AnswerDto>());
        var model = result.Model as AnswerDto;
        Assert.That(model.Id, Is.EqualTo(0));
        Assert.That(model.QuestionId, Is.EqualTo(questionId));
        Assert.That(model.Title, Is.EqualTo(""));
        Assert.That(model.IsValid, Is.False);
        _answerServiceMock.Verify(s => s.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task GetFormModal_WithNoParameters_ReturnsPartialViewWithDefaultAnswer()
    {
        // Act
        var result = await _controller.GetFormModal(null, null) as PartialViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        var model = result.Model as AnswerDto;
        Assert.That(model.QuestionId, Is.EqualTo(0));
        Assert.That(model.IsValid, Is.False);
    }

    #endregion

    #region Create Tests

    [Test]
    public async Task Create_WithValidModel_RedirectsToQuestionEdit()
    {
        // Arrange
        var answerDto = new AnswerDto { QuestionId = 5, Title = "Answer", IsValid = true, Explanation = "Explanation" };
        _controller.ControllerContext.HttpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "IsValid", "true" }
        });
        _answerServiceMock.Setup(s => s.AddAsync(It.IsAny<AnswerDto>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(answerDto) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Edit"));
        Assert.That(result.ControllerName, Is.EqualTo("Question"));
        Assert.That(result.RouteValues["id"], Is.EqualTo(5));
        _answerServiceMock.Verify(s => s.AddAsync(It.Is<AnswerDto>(a => a.IsValid == true)), Times.Once);
    }

    [Test]
    public async Task Create_WithQuestionIdZero_ReturnsBadRequest()
    {
        // Arrange
        var answerDto = new AnswerDto { QuestionId = 0, Title = "Answer", IsValid = true };
        _controller.ControllerContext.HttpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "IsValid", "true" }
        });

        // Act
        var result = await _controller.Create(answerDto) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.EqualTo("QuestionId ne peut pas être 0"));
        _answerServiceMock.Verify(s => s.AddAsync(It.IsAny<AnswerDto>()), Times.Never);
    }

    [Test]
    public async Task Create_WithInvalidIsValidCheckbox_ParsesCorrectly()
    {
        // Arrange
        var answerDto = new AnswerDto { QuestionId = 5, Title = "Answer", IsValid = false };
        _controller.ControllerContext.HttpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "IsValid", "false" }
        });
        _answerServiceMock.Setup(s => s.AddAsync(It.IsAny<AnswerDto>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(answerDto);

        // Assert
        _answerServiceMock.Verify(s => s.AddAsync(It.Is<AnswerDto>(a => a.IsValid == false)), Times.Once);
    }

    [Test]
    public async Task Create_ServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var answerDto = new AnswerDto { QuestionId = 5, Title = "Answer", IsValid = true };
        _controller.ControllerContext.HttpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "IsValid", "true" }
        });
        _answerServiceMock.Setup(s => s.AddAsync(It.IsAny<AnswerDto>())).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Create(answerDto) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.ModelState.ErrorCount, Is.GreaterThan(0));
    }

    #endregion

    #region Edit Tests

    [Test]
    public async Task Edit_WithValidModel_RedirectsToQuestionEdit()
    {
        // Arrange
        var answerDto = new AnswerDto { Id = 10, QuestionId = 5, Title = "Updated Answer", IsValid = true };
        _controller.ControllerContext.HttpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "IsValid", "true" }
        });
        _answerServiceMock.Setup(s => s.UpdateAsync(It.IsAny<AnswerDto>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Edit(answerDto) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Edit"));
        Assert.That(result.ControllerName, Is.EqualTo("Question"));
        Assert.That(result.RouteValues["id"], Is.EqualTo(5));
        _answerServiceMock.Verify(s => s.UpdateAsync(It.Is<AnswerDto>(a => a.Id == 10 && a.IsValid == true)), Times.Once);
    }

    [Test]
    public async Task Edit_WithUncheckedCheckbox_SetsIsValidToFalse()
    {
        // Arrange
        var answerDto = new AnswerDto { Id = 10, QuestionId = 5, Title = "Answer", IsValid = true };
        _controller.ControllerContext.HttpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "IsValid", "false" }
        });
        _answerServiceMock.Setup(s => s.UpdateAsync(It.IsAny<AnswerDto>())).Returns(Task.CompletedTask);

        // Act
        await _controller.Edit(answerDto);

        // Assert
        _answerServiceMock.Verify(s => s.UpdateAsync(It.Is<AnswerDto>(a => a.IsValid == false)), Times.Once);
    }

    [Test]
    public async Task Edit_ServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var answerDto = new AnswerDto { Id = 10, QuestionId = 5, Title = "Answer", IsValid = true };
        _controller.ControllerContext.HttpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "IsValid", "true" }
        });
        _answerServiceMock.Setup(s => s.UpdateAsync(It.IsAny<AnswerDto>())).ThrowsAsync(new Exception("Update failed"));

        // Act
        var result = await _controller.Edit(answerDto) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.ModelState.ErrorCount, Is.GreaterThan(0));
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task Delete_WithExistingAnswer_RedirectsToQuestionEdit()
    {
        // Arrange
        var answerId = 10;
        var questionId = 5;
        var answer = new AnswerDto { Id = answerId, QuestionId = questionId, Title = "Answer" };
        _answerServiceMock.Setup(s => s.GetByIdAsync(answerId)).ReturnsAsync(answer);
        _answerServiceMock.Setup(s => s.DeleteAsync(answerId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(answerId) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Edit"));
        Assert.That(result.ControllerName, Is.EqualTo("Question"));
        Assert.That(result.RouteValues["id"], Is.EqualTo(questionId));
        _answerServiceMock.Verify(s => s.DeleteAsync(answerId), Times.Once);
    }

    [Test]
    public async Task Delete_WithNonExistingAnswer_RedirectsToQuestionIndex()
    {
        // Arrange
        var answerId = 999;
        _answerServiceMock.Setup(s => s.GetByIdAsync(answerId)).ReturnsAsync((AnswerDto?)null);

        // Act
        var result = await _controller.Delete(answerId) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        Assert.That(result.ControllerName, Is.EqualTo("Question"));
        _answerServiceMock.Verify(s => s.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task Delete_ServiceThrowsException_RedirectsToQuestionIndex()
    {
        // Arrange
        var answerId = 10;
        _answerServiceMock.Setup(s => s.GetByIdAsync(answerId)).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Delete(answerId) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        Assert.That(result.ControllerName, Is.EqualTo("Question"));
    }

    #endregion
}