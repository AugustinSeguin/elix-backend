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
        var dto = new UserAnswerDto { UserId = 1, AnswerId = 2, IsCorrect = true };
        var entity = new UserAnswer { Id = 10, UserId = 1, AnswerId = 2, IsCorrect = true };
        _repoMock.Setup(r => r.AddUserAnswerAsync(It.IsAny<UserAnswer>())).ReturnsAsync(entity);
        _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _service.AddUserAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(10));
        Assert.That(result.UserId, Is.EqualTo(1));
        Assert.That(result.AnswerId, Is.EqualTo(2));
        Assert.That(result.IsCorrect, Is.True);
        _repoMock.Verify(r => r.AddUserAnswerAsync(It.Is<UserAnswer>(u => u.UserId == dto.UserId && u.AnswerId == dto.AnswerId && u.IsCorrect == dto.IsCorrect)), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetUserByIdAsync_ReturnsDto()
    {
        var entity = new UserAnswer { Id = 5, UserId = 3, AnswerId = 4, IsCorrect = false };
        _repoMock.Setup(r => r.GetUserAnswerByIdAsync(5)).ReturnsAsync(entity);

        var result = await _service.GetUserByIdAsync(5);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(5));
        Assert.That(result.UserId, Is.EqualTo(3));
        Assert.That(result.AnswerId, Is.EqualTo(4));
        Assert.That(result.IsCorrect, Is.False);
    }

    [Test]
    public async Task GetAllUsersAsync_ReturnsDtos()
    {
        var list = new List<UserAnswer>
        {
            new UserAnswer { Id = 1, UserId = 10, AnswerId = 11, IsCorrect = false },
            new UserAnswer { Id = 2, UserId = 12, AnswerId = 13, IsCorrect = true }
        };
        _repoMock.Setup(r => r.GetAllUserAnswersAsync()).ReturnsAsync(list);

        var result = await _service.GetAllUsersAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.Any(x => x.Id == 1 && x.UserId == 10 && x.AnswerId == 11 && x.IsCorrect == false), Is.True);
        Assert.That(result.Any(x => x.Id == 2 && x.UserId == 12 && x.AnswerId == 13 && x.IsCorrect == true), Is.True);
    }

    [Test]
    public async Task UpdateUserAsync_CallsRepositoryAndReturnsDto()
    {
        var dto = new UserAnswerDto { Id = 7, UserId = 20, AnswerId = 21, IsCorrect = true };
        var entity = new UserAnswer { Id = 7, UserId = 20, AnswerId = 21, IsCorrect = true };
        _repoMock.Setup(r => r.UpdateUserAnswerAsync(It.IsAny<UserAnswer>())).ReturnsAsync(entity);
        _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _service.UpdateUserAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(7));
        Assert.That(result.UserId, Is.EqualTo(20));
        Assert.That(result.AnswerId, Is.EqualTo(21));
        Assert.That(result.IsCorrect, Is.True);
        _repoMock.Verify(r => r.UpdateUserAnswerAsync(It.Is<UserAnswer>(u => u.Id == dto.Id && u.UserId == dto.UserId && u.AnswerId == dto.AnswerId && u.IsCorrect == dto.IsCorrect)), Times.Once);
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
}