using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ElixBackend.API.Controllers;
using ElixBackend.Business.IService;
using ElixBackend.Business.DTO;
using ElixBackend.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ElixBackend.Tests.API.Controllers;


[TestFixture]
public class UserControllerTest
{
    private Mock<IUserService> _userServiceMock;
    private Mock<IConfiguration> _configurationMock;
    private Mock<ITokenService> _tokenServiceMock;

    [SetUp]
    public void SetUp()
    {
        _userServiceMock = new Mock<IUserService>();
        _configurationMock = new Mock<IConfiguration>();
        _tokenServiceMock = new Mock<ITokenService>();
    }

    private UserController CreateController()
    {
        return new UserController(
            _userServiceMock.Object,
            _configurationMock.Object,
            _tokenServiceMock.Object
        );
    }

    [Test]
    public async Task Login_InvalidUser_ReturnsUnauthorized()
    {
        var controller = CreateController();
        var loginRequest = new LoginRequestDto { Email = "notfound@test.com", Password = "pass" };
        _userServiceMock.Setup(s => s.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync((User?)null);

        var result = await controller.Login(loginRequest);

        Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var controller = CreateController();
        var loginRequest = new LoginRequestDto { Email = "test@test.com", Password = "wrong" };
        var user = new User
        {
            Id = 1,
            Email = loginRequest.Email,
            PasswordHash = new PasswordHasher<User>().HashPassword(null,
                "good"),
            Firstname = "joe",
            Lastname = "joe"
        };
        _userServiceMock.Setup(s => s.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync(user);

        var result = await controller.Login(loginRequest);

        Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
    }
}