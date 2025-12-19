using ElixBackend.API.Controllers;
using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ElixBackend.Tests.API.Controllers;

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
    public async Task StartQuiz_ReturnsOkWithQuiz_WhenQuizExists()
    {
        // Arrange
        var userId = 1;
        var categoryId = 1;
        var expectedQuiz = new QuizDto
        {
            Id = 1,
            Title = "Quiz Test",
            CategoryId = categoryId,
            Questions = new List<QuestionDto>
            {
                new QuestionDto { Id = 1, Title = "Question 1", CategoryId = categoryId },
                new QuestionDto { Id = 2, Title = "Question 2", CategoryId = categoryId }
            }
        };

        _quizServiceMock.Setup(s => s.StartQuizAsync(userId, categoryId))
            .ReturnsAsync(expectedQuiz);

        // Act
        var result = await _controller.StartQuiz(userId, categoryId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedQuiz));
        _quizServiceMock.Verify(s => s.StartQuizAsync(userId, categoryId), Times.Once);
    }

    [Test]
    public async Task StartQuiz_ReturnsNotFound_WhenQuizDoesNotExist()
    {
        // Arrange
        var userId = 1;
        var categoryId = 999;

        _quizServiceMock.Setup(s => s.StartQuizAsync(userId, categoryId))
            .ReturnsAsync((QuizDto?)null);

        // Act
        var result = await _controller.StartQuiz(userId, categoryId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        _quizServiceMock.Verify(s => s.StartQuizAsync(userId, categoryId), Times.Once);
    }

    [Test]
    public async Task StartQuiz_ReturnsNotFound_WhenNoCategoryQuestions()
    {
        // Arrange
        var userId = 1;
        var categoryId = 2;

        _quizServiceMock.Setup(s => s.StartQuizAsync(userId, categoryId))
            .ReturnsAsync((QuizDto?)null);

        // Act
        var result = await _controller.StartQuiz(userId, categoryId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        _quizServiceMock.Verify(s => s.StartQuizAsync(userId, categoryId), Times.Once);
    }

    [Test]
    public async Task SubmitQuiz_ReturnsOk_WhenSubmissionIsValid()
    {
        // Arrange
        var quizSubmission = new QuizSubmissionDto
        {
            UserId = 1,
            CategoryId = 1,
            UserAnswers = new List<UserAnsweredQuestionDto>
            {
                new UserAnsweredQuestionDto { QuestionId = 1, AnswerIdSelected = 1 },
                new UserAnsweredQuestionDto { QuestionId = 2, AnswerIdSelected = 5 },
                new UserAnsweredQuestionDto { QuestionId = 3, AnswerIdSelected = 9 },
                new UserAnsweredQuestionDto { QuestionId = 4, AnswerIdSelected = 13 },
                new UserAnsweredQuestionDto { QuestionId = 5, AnswerIdSelected = 17 },
                new UserAnsweredQuestionDto { QuestionId = 6, AnswerIdSelected = 21 },
                new UserAnsweredQuestionDto { QuestionId = 7, AnswerIdSelected = 25 },
                new UserAnsweredQuestionDto { QuestionId = 8, AnswerIdSelected = 29 },
                new UserAnsweredQuestionDto { QuestionId = 9, AnswerIdSelected = 33 },
                new UserAnsweredQuestionDto { QuestionId = 10, AnswerIdSelected = 37 }
            }
        };

        var expectedResult = new List<CorrectionDto>
        {
            new CorrectionDto { QuestionId = 1, IsCorrect = true },
            new CorrectionDto { QuestionId = 2, IsCorrect = true }
        };

        _quizServiceMock.Setup(s => s.SubmitQuizAsync(quizSubmission))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.SubmitQuiz(quizSubmission);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedResult));
        _quizServiceMock.Verify(s => s.SubmitQuizAsync(quizSubmission), Times.Once);
    }

    [Test]
    public async Task SubmitQuiz_ReturnsBadRequest_WhenUserAnswersIsEmpty()
    {
        // Arrange
        var quizSubmission = new QuizSubmissionDto
        {
            UserId = 1,
            CategoryId = 1,
            UserAnswers = new List<UserAnsweredQuestionDto>()
        };

        // Act
        var result = await _controller.SubmitQuiz(quizSubmission);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        _quizServiceMock.Verify(s => s.SubmitQuizAsync(It.IsAny<QuizSubmissionDto>()), Times.Never);
    }

    [Test]
    public async Task SubmitQuiz_ReturnsNotFound_WhenQuizNotFound()
    {
        // Arrange
        var quizSubmission = new QuizSubmissionDto
        {
            UserId = 1,
            CategoryId = 999,
            UserAnswers = new List<UserAnsweredQuestionDto>
            {
                new UserAnsweredQuestionDto { QuestionId = 1, AnswerIdSelected = 1 }
            }
        };

        _quizServiceMock.Setup(s => s.SubmitQuizAsync(quizSubmission))
            .ReturnsAsync((List<CorrectionDto>?)null);

        // Act
        var result = await _controller.SubmitQuiz(quizSubmission);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        _quizServiceMock.Verify(s => s.SubmitQuizAsync(quizSubmission), Times.Once);
    }

    [Test]
    public async Task SubmitQuiz_CallsServiceWithCorrectData()
    {
        // Arrange
        var quizSubmission = new QuizSubmissionDto
        {
            UserId = 2,
            CategoryId = 3,
            UserAnswers = new List<UserAnsweredQuestionDto>
            {
                new UserAnsweredQuestionDto { QuestionId = 10, AnswerIdSelected = 40 },
                new UserAnsweredQuestionDto { QuestionId = 11, AnswerIdSelected = 44 }
            }
        };

        var expectedResult = new List<CorrectionDto>
        {
            new CorrectionDto { QuestionId = 10, IsCorrect = true },
            new CorrectionDto { QuestionId = 11, IsCorrect = false }
        };

        _quizServiceMock.Setup(s => s.SubmitQuizAsync(It.Is<QuizSubmissionDto>(
                q => q.UserId == 2 && q.CategoryId == 3 && q.UserAnswers.Count == 2)))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.SubmitQuiz(quizSubmission);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        _quizServiceMock.Verify(s => s.SubmitQuizAsync(It.Is<QuizSubmissionDto>(
            q => q.UserId == 2 && q.CategoryId == 3 && q.UserAnswers.Count == 2)), Times.Once);
    }

    [Test]
    public async Task StartQuiz_WithDifferentUserIds_CallsServiceCorrectly()
    {
        // Arrange
        var userId1 = 1;
        var userId2 = 2;
        var categoryId = 1;

        var quiz1 = new QuizDto { Id = 1, Title = "Quiz User 1", CategoryId = categoryId };
        var quiz2 = new QuizDto { Id = 2, Title = "Quiz User 2", CategoryId = categoryId };

        _quizServiceMock.Setup(s => s.StartQuizAsync(userId1, categoryId)).ReturnsAsync(quiz1);
        _quizServiceMock.Setup(s => s.StartQuizAsync(userId2, categoryId)).ReturnsAsync(quiz2);

        // Act
        var result1 = await _controller.StartQuiz(userId1, categoryId);
        var result2 = await _controller.StartQuiz(userId2, categoryId);

        // Assert
        var okResult1 = result1.Result as OkObjectResult;
        var okResult2 = result2.Result as OkObjectResult;

        Assert.That(okResult1?.Value, Is.EqualTo(quiz1));
        Assert.That(okResult2?.Value, Is.EqualTo(quiz2));
        _quizServiceMock.Verify(s => s.StartQuizAsync(userId1, categoryId), Times.Once);
        _quizServiceMock.Verify(s => s.StartQuizAsync(userId2, categoryId), Times.Once);
    }
}
