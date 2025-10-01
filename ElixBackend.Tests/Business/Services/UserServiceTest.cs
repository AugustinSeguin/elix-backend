using ElixBackend.Business.DTO;
using ElixBackend.Business.Service;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Moq;

namespace ElixBackend.Tests.Business.Services;

[TestFixture]
public class UserServiceTest
{
    private Mock<IUserRepository> _userRepositoryMock;
    private UserService _userService;

    [SetUp]
    public void SetUp()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userService = new UserService(_userRepositoryMock.Object);
    }

    [Test]
    public async Task GetUserByIdAsync_ReturnsUser()
    {
        var user = new User { Id = 1, Firstname = "John", Lastname = "Doe", Email = "john@doe.com", PasswordHash = "hash" };
        _userRepositoryMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);

        var result = await _userService.GetUserByIdAsync(1);

        Assert.That(result, Is.EqualTo(user));
    }

    [Test]
    public async Task GetUserByEmailAsync_ReturnsUser()
    {
        var user = new User { Id = 2, Firstname = "Jane", Lastname = "Smith", Email = "jane@smith.com", PasswordHash = "hash" };
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync("jane@smith.com")).ReturnsAsync(user);

        var result = await _userService.GetUserByEmailAsync("jane@smith.com");

        Assert.That(result, Is.EqualTo(user));
    }

    [Test]
    public async Task GetAllUsersAsync_ReturnsUsers()
    {
        var users = new List<User>
        {
            new User { Id = 1, Firstname = "John", Lastname = "Doe", Email = "john@doe.com", PasswordHash = "hash" },
            new User { Id = 2, Firstname = "Jane", Lastname = "Smith", Email = "jane@smith.com", PasswordHash = "hash" }
        };
        _userRepositoryMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(users);

        var result = await _userService.GetAllUsersAsync();

        Assert.That(result, Is.EquivalentTo(users));
    }

    [Test]
    public async Task AddUserAsync_CallsRepositoryAndReturnsUser()
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

        Assert.That(result, Is.EqualTo(user));
        _userRepositoryMock.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateUserAsync_CallsRepositoryAndReturnsUser()
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

        Assert.That(result, Is.EqualTo(user));
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
}