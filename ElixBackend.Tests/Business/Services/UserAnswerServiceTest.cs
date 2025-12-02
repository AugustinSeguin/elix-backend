using ElixBackend.Business.DTO;
using ElixBackend.Business.Service;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Moq;

namespace ElixBackend.Tests.Business.Services;

[TestFixture]
public class UserAnswerServiceTest
{
    private Mock<IUserAnswerRepository> _repoMock;
    private UserAnswerService _service;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IUserAnswerRepository>();
        _service = new UserAnswerService(_repoMock.Object);
    }

    [Test]
    public async Task AddUserAsync_CallsRepositoryAndReturnsDto()
    {
        var dto = new UserAnswerDto { UserId = 1, QuestionId = 2, IsCorrect = true };
        var entity = new UserAnswer { Id = 10, UserId = 1, QuestionId = 2, IsCorrect = true };
        _repoMock.Setup(r => r.AddUserAnswerAsync(It.IsAny<UserAnswer>())).ReturnsAsync(entity);
        _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _service.AddUserAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(10));
        Assert.That(result.UserId, Is.EqualTo(1));
        Assert.That(result.QuestionId, Is.EqualTo(2));
        Assert.That(result.IsCorrect, Is.True);
        _repoMock.Verify(r => r.AddUserAnswerAsync(It.Is<UserAnswer>(u => u.UserId == dto.UserId && u.QuestionId == dto.QuestionId && u.IsCorrect == dto.IsCorrect)), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateUserAsync_CallsRepositoryAndReturnsDto()
    {
        var dto = new UserAnswerDto { Id = 7, UserId = 20, QuestionId = 21, IsCorrect = true };
        var entity = new UserAnswer { Id = 7, UserId = 20, QuestionId = 21, IsCorrect = true };
        _repoMock.Setup(r => r.UpdateUserAnswerAsync(It.IsAny<UserAnswer>())).ReturnsAsync(entity);
        _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _service.UpdateUserAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(7));
        Assert.That(result.UserId, Is.EqualTo(20));
        Assert.That(result.QuestionId, Is.EqualTo(21));
        Assert.That(result.IsCorrect, Is.True);
        _repoMock.Verify(r => r.UpdateUserAnswerAsync(It.Is<UserAnswer>(u => u.Id == dto.Id && u.UserId == dto.UserId && u.QuestionId == dto.QuestionId && u.IsCorrect == dto.IsCorrect)), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteUserAsync_CallsRepository()
    {
        _repoMock.Setup(r => r.DeleteUserAnswerAsync(9)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        await _service.DeleteUserAsync(9);

        _repoMock.Verify(r => r.DeleteUserAnswerAsync(9), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetUserAnswerByUserIdAsync_ReturnsFilteredDtos()
    {
        var allUserAnswers = new List<UserAnswer>
        {
            new UserAnswer { Id = 1, UserId = 5, QuestionId = 10, IsCorrect = true },
            new UserAnswer { Id = 2, UserId = 5, QuestionId = 11, IsCorrect = false },
            new UserAnswer { Id = 3, UserId = 6, QuestionId = 10, IsCorrect = true },
            new UserAnswer { Id = 4, UserId = 5, QuestionId = 10, IsCorrect = false }
        };
        _repoMock.Setup(r => r.GetUserAnswerByUserIdAsync(5, 10)).ReturnsAsync(
            allUserAnswers.Where(ua => ua.UserId == 5 && ua.QuestionId == 10));

        var result = await _service.GetUserAnswerByUserIdAsync(5, 10);
        var listResult = result.Where(r => r != null).ToList();

        Assert.That(listResult.Count, Is.EqualTo(2));
        Assert.That(listResult.All(ua => ua!.UserId == 5 && ua.QuestionId == 10), Is.True);
        Assert.That(listResult.Any(ua => ua!.IsCorrect), Is.True);
        Assert.That(listResult.Any(ua => !ua!.IsCorrect), Is.True);
        _repoMock.Verify(r => r.GetUserAnswerByUserIdAsync(5, 10), Times.Once);
    }
}