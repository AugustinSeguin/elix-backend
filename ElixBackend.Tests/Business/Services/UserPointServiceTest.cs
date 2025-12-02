using ElixBackend.Business.DTO;
using ElixBackend.Business.Service;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Moq;

namespace ElixBackend.Tests.Business.Services;

[TestFixture]
public class UserPointServiceTest
{
    private Mock<IUserPointRepository> _repoMock;
    private UserPointService _service;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IUserPointRepository>();
        _service = new UserPointService(_repoMock.Object);
    }

    [Test]
    public async Task AddUserAsync_CallsRepositoryAndReturnsDto()
    {
        var dto = new UserPointDto { UserId = 1, CategoryId = 2, Points = 10 };
        var entity = new UserPoint { Id = 100, UserId = 1, CategoryId = 2, Points = 10 };
        _repoMock.Setup(r => r.AddUserPointAsync(It.IsAny<UserPoint>())).ReturnsAsync(entity);
        _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _service.AddUserPointAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(100));
        Assert.That(result.UserId, Is.EqualTo(1));
        Assert.That(result.CategoryId, Is.EqualTo(2));
        Assert.That(result.Points, Is.EqualTo(10));
        _repoMock.Verify(r => r.AddUserPointAsync(It.Is<UserPoint>(u => u.UserId == dto.UserId && u.CategoryId == dto.CategoryId && u.Points == dto.Points)), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetUserByIdAsync_ReturnsDto()
    {
        var entity = new UserPoint { Id = 5, UserId = 3, CategoryId = 4, Points = 7 };
        _repoMock.Setup(r => r.GetUserPointByIdAsync(5)).ReturnsAsync(entity);

        var result = await _service.GetUserByIdAsync(5);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(5));
        Assert.That(result.UserId, Is.EqualTo(3));
        Assert.That(result.CategoryId, Is.EqualTo(4));
        Assert.That(result.Points, Is.EqualTo(7));
    }

    [Test]
    public async Task GetAllUsersAsync_ReturnsDtos()
    {
        var list = new List<UserPoint>
        {
            new UserPoint { Id = 1, UserId = 10, CategoryId = 11, Points = 1 },
            new UserPoint { Id = 2, UserId = 12, CategoryId = 13, Points = 2 }
        };
        _repoMock.Setup(r => r.GetAllUserPointsAsync()).ReturnsAsync(list);

        var result = await _service.GetAllUserPointsAsync();
        var asList = result.ToList();

        Assert.That(asList.Count, Is.EqualTo(2));
        Assert.That(asList.Any(x => x.Id == 1 && x.UserId == 10 && x.CategoryId == 11 && x.Points == 1));
        Assert.That(asList.Any(x => x.Id == 2 && x.UserId == 12 && x.CategoryId == 13 && x.Points == 2));
    }

    [Test]
    public async Task UpdateUserAsync_CallsRepositoryAndReturnsDto()
    {
        var dto = new UserPointDto { Id = 7, UserId = 20, CategoryId = 21, Points = 8 };
        var entity = new UserPoint { Id = 7, UserId = 20, CategoryId = 21, Points = 8 };
        _repoMock.Setup(r => r.UpdateUserPointAsync(It.IsAny<UserPoint>())).ReturnsAsync(entity);
        _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _service.UpdateUserPointAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(7));
        Assert.That(result.UserId, Is.EqualTo(20));
        Assert.That(result.CategoryId, Is.EqualTo(21));
        Assert.That(result.Points, Is.EqualTo(8));
        _repoMock.Verify(r => r.UpdateUserPointAsync(It.Is<UserPoint>(u => u.Id == dto.Id && u.UserId == dto.UserId && u.CategoryId == dto.CategoryId && u.Points == dto.Points)), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteUserAsync_CallsRepository()
    {
        _repoMock.Setup(r => r.DeleteUserPointAsync(9)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        await _service.DeleteUserPointAsync(9);

        _repoMock.Verify(r => r.DeleteUserPointAsync(9), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}