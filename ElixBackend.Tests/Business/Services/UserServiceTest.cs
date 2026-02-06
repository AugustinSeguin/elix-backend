using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Business.Service;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.Extensions.Logging;
using Moq;

namespace ElixBackend.Tests.Business.Services;

[TestFixture]
public class UserServiceTest
{
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IUserPointService> _userPointServiceMock;
    private Mock<ILogger<UserService>> _loggerMock;
    private UserService _userService;

    [SetUp]
    public void SetUp()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userPointServiceMock = new Mock<IUserPointService>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _userService = new UserService(_userRepositoryMock.Object, _userPointServiceMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task GetUserByIdAsync_ReturnsUserDto()
    {
        var user = new User { Id = 1, Firstname = "John", Lastname = "Doe", Email = "john@doe.com", PasswordHash = "hash" };
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);

        var result = await _userService.GetUserByIdAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(user.Id));
        Assert.That(result.Email, Is.EqualTo(user.Email));
    }

    [Test]
    public async Task GetUserByEmailAsync_ReturnsUserDto()
    {
        var user = new User { Id = 2, Firstname = "Jane", Lastname = "Smith", Email = "jane@smith.com", PasswordHash = "hash" };
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync("jane@smith.com")).ReturnsAsync(user);

        var result = await _userService.GetUserByEmailAsync("jane@smith.com");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Email, Is.EqualTo(user.Email));
    }

    [Test]
    public async Task GetAllUsersAsync_ReturnsUserDtos()
    {
        var users = new List<User>
        {
            new User { Id = 1, Firstname = "John", Lastname = "Doe", Email = "john@doe.com", PasswordHash = "hash" },
            new User { Id = 2, Firstname = "Jane", Lastname = "Smith", Email = "jane@smith.com", PasswordHash = "hash" }
        };
        _userRepositoryMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(users);

        var result = await _userService.GetAllUsersAsync();

        Assert.That(result, Is.Not.Null);
        var asList = result!.ToList();
        Assert.That(asList.Count, Is.EqualTo(2));
        Assert.That(asList.Any(d => d.Email == "john@doe.com"), Is.True);
        Assert.That(asList.Any(d => d.Email == "jane@smith.com"), Is.True);
    }

    [Test]
    public async Task AddUserAsync_CallsRepositoryAndReturnsUserDto()
    {
        var userDto = new UserDto
        {
            Firstname = "Alice",
            Lastname = "Wonder",
            Email = "alice@wonder.com",
            Password = "pwd",
            PasswordRepeated =  "pwd"
        };
        var user = new User { Id = 3, Firstname = "Alice", Lastname = "Wonder", Email = "alice@wonder.com", PasswordHash = "hash" };

        _userRepositoryMock.Setup(r => r.AddUserAsync(It.IsAny<User>())).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _userService.AddUserAsync(userDto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(user.Id));
        _userRepositoryMock.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateUserAsync_CallsRepositoryAndReturnsUserDto()
    {
        var userDto = new UserDto
        {
            Id = 4,
            Firstname = "Bob",
            Lastname = "Builder",
            Email = "bob@builder.com",
            Password = "pwd",
            PasswordRepeated =  "pwd"
        };
        var user = new User { Id = 4, Firstname = "Bob", Lastname = "Builder", Email = "bob@builder.com", PasswordHash = "hash" };

        _userRepositoryMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var result = await _userService.UpdateUserAsync(userDto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(user.Id));
        _userRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteUserAsync_CallsRepository()
    {
        _userRepositoryMock.Setup(r => r.DeleteUserAsync(5)).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        await _userService.DeleteUserAsync(5);

        _userRepositoryMock.Verify(r => r.DeleteUserAsync(5), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetMeAsync_ReturnsUserDto()
    {
        var user = new User { Id = 6, Firstname = "Charlie", Lastname = "Brown", Email = "charlie@brown.com", PasswordHash = "hash" };
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(6)).ReturnsAsync(user);
        _userPointServiceMock.Setup(s => s.GetUserPoints(6)).ReturnsAsync(new List<UserPointDto>());
        _userPointServiceMock.Setup(s => s.GetTotalPointsByUserIdAsync(6)).ReturnsAsync(0);

        var result = await _userService.GetMeAsync(6);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(user.Id));
        Assert.That(result.Email, Is.EqualTo(user.Email));
        Assert.That(result.Firstname, Is.EqualTo(user.Firstname));
        Assert.That(result.UserPoints, Is.Not.Null);
        Assert.That(result.BadgeUrl, Is.EqualTo("/beginner.png"));
        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(6), Times.Once);
    }

    [Test]
    public async Task GetMeAsync_ReturnsAdvancedBadge_WhenPointsAboveAdvanced()
    {
        var user = new User { Id = 7, Firstname = "Dana", Lastname = "Doe", Email = "dana@doe.com", PasswordHash = "hash" };
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(7)).ReturnsAsync(user);
        _userPointServiceMock.Setup(s => s.GetUserPoints(7)).ReturnsAsync(new List<UserPointDto>());
        _userPointServiceMock.Setup(s => s.GetTotalPointsByUserIdAsync(7)).ReturnsAsync(200);

        var result = await _userService.GetMeAsync(7);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.BadgeUrl, Is.EqualTo("/advanced.png"));
    }

    [Test]
    public async Task GetMeAsync_ReturnsNull_WhenUserNotFound()
    {
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(999)).ReturnsAsync((User?)null);

        var result = await _userService.GetMeAsync(999);

        Assert.That(result, Is.Null);
        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(999), Times.Once);
    }
}