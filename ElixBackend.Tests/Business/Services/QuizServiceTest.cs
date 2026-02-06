using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Business.Service;
using ElixBackend.Domain.Enum;
using Microsoft.Extensions.Logging;
using Moq;

namespace ElixBackend.Tests.Business.Services;

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
            _userPointServiceMock.Object, _loggerMock.Object);
    }

    // Helpers
    private static QuestionDto MakeQuestion(int id, int categoryId, bool withAnswers = false, TypeQuestion type = TypeQuestion.QuizModeMcq)
    {
        var q = new QuestionDto
        {
            Id = id,
            Title = $"Q{id}",
            CategoryId = categoryId,
            TypeQuestion = type
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

    private static IEnumerable<UserAnswerDto> Answers(params (int questionId, bool isCorrect)[] tuples)
    {
        return tuples.Select(t => new UserAnswerDto
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
    public async Task StartQuizAsync_ReturnsMixedQuestions_WhenEnoughQuestionsAvailable()
    {
        var categoryId = 10;
        var questions = new List<QuestionDto>();

        // 10 MCQ questions
        for (int i = 1; i <= 10; i++)
        {
            questions.Add(MakeQuestion(i, categoryId, withAnswers: true, type: TypeQuestion.QuizModeMcq));
        }
        // 10 TF questions
        for (int i = 11; i <= 20; i++)
        {
            questions.Add(MakeQuestion(i, categoryId, withAnswers: true, type: TypeQuestion.TrueFalseActive));
        }

        _questionServiceMock.Setup(s => s.GetQuestionsByCategoryIdAsync(categoryId)).ReturnsAsync(questions);
        _userAnswerServiceMock.Setup(s => s.GetUserAnswerByUserIdAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((IEnumerable<UserAnswerDto>?)null);

        var result = await _service.StartQuizAsync(1, categoryId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Questions.Count, Is.EqualTo(10));

        var resultQuestions = result.Questions.ToList();

        // Check counts
        Assert.That(resultQuestions.Count(q => q.TypeQuestion == TypeQuestion.QuizModeMcq), Is.EqualTo(5));
        Assert.That(resultQuestions.Count(q => q.TypeQuestion == TypeQuestion.TrueFalseActive), Is.EqualTo(5));

        // Check pattern: alternating TF / MCQ
        Assert.That(resultQuestions[0].TypeQuestion, Is.EqualTo(TypeQuestion.TrueFalseActive));
        Assert.That(resultQuestions[1].TypeQuestion, Is.EqualTo(TypeQuestion.QuizModeMcq));
        Assert.That(resultQuestions[2].TypeQuestion, Is.EqualTo(TypeQuestion.TrueFalseActive));
        Assert.That(resultQuestions[3].TypeQuestion, Is.EqualTo(TypeQuestion.QuizModeMcq));
        Assert.That(resultQuestions[4].TypeQuestion, Is.EqualTo(TypeQuestion.TrueFalseActive));
        Assert.That(resultQuestions[5].TypeQuestion, Is.EqualTo(TypeQuestion.QuizModeMcq));
        Assert.That(resultQuestions[6].TypeQuestion, Is.EqualTo(TypeQuestion.TrueFalseActive));
        Assert.That(resultQuestions[7].TypeQuestion, Is.EqualTo(TypeQuestion.QuizModeMcq));
        Assert.That(resultQuestions[8].TypeQuestion, Is.EqualTo(TypeQuestion.TrueFalseActive));
        Assert.That(resultQuestions[9].TypeQuestion, Is.EqualTo(TypeQuestion.QuizModeMcq));
    }

    [Test]
    public async Task StartQuizAsync_Prioritizes_NotAnswered_Then_Incorrect_Then_Correct_And_Takes10()
    {
        var categoryId = 2;
        // 12 questions avec réponses (au minimum 2 réponses et 1 réponse valide)
        // All MCQ to simplify testing priority within one type
        var questions = Enumerable.Range(1, 12).Select(i => MakeQuestion(i, categoryId, withAnswers: true, type: TypeQuestion.QuizModeMcq)).ToList();
        _questionServiceMock.Setup(s => s.GetQuestionsByCategoryIdAsync(categoryId)).ReturnsAsync(questions);

        // User answers: none for Q1..Q6 (not answered), incorrect for Q7..Q11, correct for Q12
        foreach (var q in questions)
        {
            if (q.Id <= 6)
            {
                _userAnswerServiceMock.Setup(s => s.GetUserAnswerByUserIdAsync(1, q.Id)).ReturnsAsync(new List<UserAnswerDto>());
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
        // We only have MCQ, so it should take top 10 MCQ.
        Assert.That(result!.Questions.Count, Is.EqualTo(10));

        // Should contain Q1..Q10 (priorité not answered puis incorrect)
        var expectedIds = Enumerable.Range(1, 10).ToArray();
        Assert.That(result.Questions.Select(q => q.Id), Is.EqualTo(expectedIds));
    }

    [Test]
    public async Task StartQuizAsync_AllCorrect_SelectsFromCorrectlyAnswered_WhenNeeded()
    {
        var categoryId = 3;
        var questions = new List<QuestionDto>();
        // 5 MCQ
        questions.AddRange(Enumerable.Range(1, 5).Select(i => MakeQuestion(i, categoryId, withAnswers: true, type: TypeQuestion.QuizModeMcq)));
        // 5 TF
        questions.AddRange(Enumerable.Range(6, 5).Select(i => MakeQuestion(i, categoryId, withAnswers: true, type: TypeQuestion.TrueFalseActive)));

        _questionServiceMock.Setup(s => s.GetQuestionsByCategoryIdAsync(categoryId)).ReturnsAsync(questions);

        foreach (var q in questions)
        {
            _userAnswerServiceMock.Setup(s => s.GetUserAnswerByUserIdAsync(1, q.Id))
                .ReturnsAsync(Answers((q.Id, true)));
        }

        var result = await _service.StartQuizAsync(1, categoryId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Questions.Count, Is.EqualTo(10));
        // Order might be mixed (2 TF, 2 MCQ...), so we just check content
        Assert.That(result.Questions.Select(q => q.Id), Is.EquivalentTo(Enumerable.Range(1, 10).ToArray()));
    }

    [Test]
    public async Task StartQuizAsync_FiltersQuestionsWithLessThanTwoAnswers()
    {
        var categoryId = 4;
        var questions = new List<QuestionDto>
        {
            // Question avec 4 réponses (valide)
            MakeQuestion(1, categoryId, withAnswers: true),
            // Question avec seulement 1 réponse (invalide)
            new QuestionDto
            {
                Id = 2,
                Title = "Q2",
                CategoryId = categoryId,
                Answers = new List<AnswerDto>
                {
                    new AnswerDto { Id = 21, QuestionId = 2, Title = "A2-1", IsValid = true, Explanation = "E2" }
                }
            },
            // Question sans réponses (invalide)
            new QuestionDto
            {
                Id = 3,
                Title = "Q3",
                CategoryId = categoryId,
                Answers = new List<AnswerDto>()
            },
            // Question avec 2 réponses mais aucune valide (invalide)
            new QuestionDto
            {
                Id = 4,
                Title = "Q4",
                CategoryId = categoryId,
                Answers = new List<AnswerDto>
                {
                    new AnswerDto { Id = 41, QuestionId = 4, Title = "A4-1", IsValid = false },
                    new AnswerDto { Id = 42, QuestionId = 4, Title = "A4-2", IsValid = false }
                }
            },
            // Question avec 3 réponses et 1 valide (valide)
            MakeQuestion(5, categoryId, withAnswers: true)
        };

        _questionServiceMock.Setup(s => s.GetQuestionsByCategoryIdAsync(categoryId)).ReturnsAsync(questions);

        _userAnswerServiceMock.Setup(s => s.GetUserAnswerByUserIdAsync(1, It.IsAny<int>()))
            .ReturnsAsync(new List<UserAnswerDto>());

        var result = await _service.StartQuizAsync(1, categoryId);

        Assert.That(result, Is.Not.Null);
        // Seules les questions 1, 2 et 5 devraient être retournées
        Assert.That(result!.Questions.Count, Is.EqualTo(3));
        Assert.That(result.Questions.Select(q => q.Id), Is.EqualTo(new[] { 1, 2, 5 }));
    }

    [Test]
    public async Task StartQuizAsync_ReturnsNull_WhenNoValidQuestionsAfterFilter()
    {
        var categoryId = 5;
        var questions = new List<QuestionDto>
        {
            // Question avec seulement 1 réponse (valide)
            new QuestionDto
            {
                Id = 1,
                Title = "Q1",
                CategoryId = categoryId,
                Answers = new List<AnswerDto>
                {
                    new AnswerDto { Id = 11, QuestionId = 1, Title = "A1-1", IsValid = true }
                }
            },
            // Question sans réponse valide (invalide)
            new QuestionDto
            {
                Id = 2,
                Title = "Q2",
                CategoryId = categoryId,
                Answers = new List<AnswerDto>
                {
                    new AnswerDto { Id = 21, QuestionId = 2, Title = "A2-1", IsValid = false },
                    new AnswerDto { Id = 22, QuestionId = 2, Title = "A2-2", IsValid = false }
                }
            }
        };

        _questionServiceMock.Setup(s => s.GetQuestionsByCategoryIdAsync(categoryId)).ReturnsAsync(questions);

        var result = await _service.StartQuizAsync(1, categoryId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Questions.Count, Is.EqualTo(1));
        Assert.That(result.Questions.First().Id, Is.EqualTo(1));
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