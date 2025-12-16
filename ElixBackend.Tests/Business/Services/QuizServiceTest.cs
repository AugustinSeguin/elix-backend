using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Business.Service;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;

namespace ElixBackend.Tests.Business.Services;

[TestFixture]
public class QuizServiceTest
{
    private Mock<IQuestionService> _questionServiceMock;
    private Mock<IUserAnswerService> _userAnswerServiceMock;
    private Mock<IUserPointService> _userPointServiceMock;
    private QuizService _service;
    private Mock<ILogger<QuizService>> _loggerMock;

    [SetUp]
    public void SetUp()
    {
        _questionServiceMock = new Mock<IQuestionService>();
        _userAnswerServiceMock = new Mock<IUserAnswerService>();
        _userPointServiceMock = new Mock<IUserPointService>();
        _loggerMock = new Mock<ILogger<QuizService>>();
        _service = new QuizService(_questionServiceMock.Object, _userAnswerServiceMock.Object,
            _userPointServiceMock.Object, _loggerMock?.Object);
    }

    // Helpers
    private static QuestionDto MakeQuestion(int id, int categoryId, bool withAnswers = false)
    {
        var q = new QuestionDto
        {
            Id = id,
            Title = $"Q{id}",
            CategoryId = categoryId
        };
        if (withAnswers)
        {
            // 4 réponses dont 1 correcte
            q.Answers = new List<AnswerDto>
            {
                new AnswerDto { Id = id * 10 + 1, QuestionId = id, Title = $"A{id}-1", IsValid = true, Explanation = $"E{id}" },
                new AnswerDto { Id = id * 10 + 2, QuestionId = id, Title = $"A{id}-2", IsValid = false },
                new AnswerDto { Id = id * 10 + 3, QuestionId = id, Title = $"A{id}-3", IsValid = false },
                new AnswerDto { Id = id * 10 + 4, QuestionId = id, Title = $"A{id}-4", IsValid = false },
            };
        }
        return q;
    }

    private static IEnumerable<UserAnswerDto?> Answers(params (int questionId, bool isCorrect)[] tuples)
    {
        return tuples.Select(t => (UserAnswerDto?)new UserAnswerDto
        {
            UserId = 1,
            QuestionId = t.questionId,
            IsCorrect = t.isCorrect
        }).ToList();
    }

    [Test]
    public async Task StartQuizAsync_NoQuestions_ReturnsNull()
    {
        _questionServiceMock.Setup(s => s.GetQuestionsByCategoryIdAsync(1)).ReturnsAsync(new List<QuestionDto>());

        var result = await _service.StartQuizAsync(1, 1);

        Assert.That(result, Is.Null);
        _questionServiceMock.Verify(s => s.GetQuestionsByCategoryIdAsync(1), Times.Once);
    }

    [Test]
    public async Task StartQuizAsync_Prioritizes_NotAnswered_Then_Incorrect_Then_Correct_And_Takes10()
    {
        var categoryId = 2;
        // 12 questions
        var questions = Enumerable.Range(1, 12).Select(i => MakeQuestion(i, categoryId)).ToList();
        _questionServiceMock.Setup(s => s.GetQuestionsByCategoryIdAsync(categoryId)).ReturnsAsync(questions);

        // User answers: none for Q1..Q6 (not answered), incorrect for Q7..Q11, correct for Q12
        foreach (var q in questions)
        {
            if (q.Id <= 6)
            {
                _userAnswerServiceMock.Setup(s => s.GetUserAnswerByUserIdAsync(1, q.Id)).ReturnsAsync(new List<UserAnswerDto?>());
            }
            else if (q.Id <= 11)
            {
                _userAnswerServiceMock.Setup(s => s.GetUserAnswerByUserIdAsync(1, q.Id))
                    .ReturnsAsync(Answers((q.Id, false)));
            }
            else
            {
                _userAnswerServiceMock.Setup(s => s.GetUserAnswerByUserIdAsync(1, q.Id))
                    .ReturnsAsync(Answers((q.Id, true)));
            }
        }

        var result = await _service.StartQuizAsync(1, categoryId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Questions.Count, Is.EqualTo(10));
        // Should contain Q1..Q6 (not answered) and then 4 from incorrect Q7..Q10
        var expectedIds = Enumerable.Range(1, 6).Concat(Enumerable.Range(7, 4)).ToArray();
        CollectionAssert.AreEqual(expectedIds, result.Questions.Select(q => q.Id).ToArray());
    }

    [Test]
    public async Task StartQuizAsync_AllCorrect_SelectsFromCorrectlyAnswered_WhenNeeded()
    {
        var categoryId = 3;
        var questions = Enumerable.Range(1, 10).Select(i => MakeQuestion(i, categoryId)).ToList();
        _questionServiceMock.Setup(s => s.GetQuestionsByCategoryIdAsync(categoryId)).ReturnsAsync(questions);

        foreach (var q in questions)
        {
            _userAnswerServiceMock.Setup(s => s.GetUserAnswerByUserIdAsync(1, q.Id))
                .ReturnsAsync(Answers((q.Id, true)));
        }

        var result = await _service.StartQuizAsync(1, categoryId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Questions.Count, Is.EqualTo(10));
        CollectionAssert.AreEqual(Enumerable.Range(1, 10).ToArray(), result.Questions.Select(q => q.Id).ToArray());
    }

    [Test]
    public async Task SubmitQuizAsync_TenAnswers_EightCorrect_AddsPoints_EchoesResult()
    {
        var userId = 5; var categoryId = 9;
        // Make 10 questions with answers
        var questions = Enumerable.Range(1, 10).Select(i => MakeQuestion(i, categoryId, withAnswers: true)).ToList();
        _questionServiceMock.Setup(s => s.GetQuestionsByCategoryIdAsync(categoryId)).ReturnsAsync(questions);

        var submission = new QuizSubmissionDto
        {
            UserId = userId,
            CategoryId = categoryId,
            UserAnswers = new List<UserAnsweredQuestionDto>()
        };
        // Pick correct answers for first 8, incorrect for last 2
        foreach (var q in questions)
        {
            var correctId = q.Answers!.First(a => a.IsValid).Id;
            var chosen = q.Id <= 8 ? correctId : correctId + 1; // incorrect for last 2
            submission.UserAnswers.Add(new UserAnsweredQuestionDto { QuestionId = q.Id, AnswerIdSelected = chosen });
        }

        // Capture saved user answers
        var savedAnswers = new List<UserAnswerDto>();
        _userAnswerServiceMock
            .Setup(s => s.AddUserAsync(It.IsAny<UserAnswerDto>()))
            .ReturnsAsync((UserAnswerDto dto) => dto)
            .Callback<UserAnswerDto>(ua => savedAnswers.Add(ua));

        // userPointService.AddUserPointAsync will be called with 8 points
        _userPointServiceMock
            .Setup(s => s.AddUserPointAsync(It.Is<UserPointDto>(p => p.UserId == userId && p.CategoryId == categoryId && p.Points == 8)))
            .ReturnsAsync((UserPointDto up) => up);

        var result = await _service.SubmitQuizAsync(submission);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CategoryId, Is.EqualTo(categoryId));
        StringAssert.Contains("8/10", result.Title);

        // Ensure 10 answers saved with correct flags
        Assert.That(savedAnswers.Count, Is.EqualTo(10));
        var correctCount = savedAnswers.Count(a => a.IsCorrect);
        Assert.That(correctCount, Is.EqualTo(8));

        _userPointServiceMock.Verify(s => s.AddUserPointAsync(It.Is<UserPointDto>(p => p.Points == 8)), Times.Once);
    }

    [Test]
    public async Task SubmitQuizAsync_LessThanTen_NoPoints()
    {
        var userId = 6; var categoryId = 4;
        var questions = Enumerable.Range(1, 5).Select(i => MakeQuestion(i, categoryId, withAnswers: true)).ToList();
        _questionServiceMock.Setup(s => s.GetQuestionsByCategoryIdAsync(categoryId)).ReturnsAsync(questions);

        var submission = new QuizSubmissionDto
        {
            UserId = userId,
            CategoryId = categoryId,
            UserAnswers = questions.Select(q => new UserAnsweredQuestionDto
            {
                QuestionId = q.Id,
                AnswerIdSelected = q.Answers!.First(a => a.IsValid).Id // all correct but only 5
            }).ToList()
        };

        _userAnswerServiceMock
            .Setup(s => s.AddUserAsync(It.IsAny<UserAnswerDto>()))
            .ReturnsAsync((UserAnswerDto dto) => dto);

        var result = await _service.SubmitQuizAsync(submission);

        Assert.That(result, Is.Not.Null);
        _userPointServiceMock.Verify(s => s.AddUserPointAsync(It.IsAny<UserPointDto>()), Times.Never);
    }
}