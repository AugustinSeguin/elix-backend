using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ElixBackend.Business.IService;
using ElixBackend.Business.DTO;
using ElixBackend.Domain.Entities;
using ElixBackend.WebApp.Controllers;

namespace ElixBackend.Tests.WebApp.Controllers;

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
    public void Login_Get_ReturnsView()
    {
        var controller = CreateController();

        var result = controller.Login();

        Assert.That(result, Is.TypeOf<ViewResult>());
    }

    [Test]
    public async Task Login_Post_InvalidUser_ReturnsViewWithError()
    {
        var controller = CreateController();
        var loginRequest = new LoginRequest { Email = "test@test.com", Password = "wrong" };
        _userServiceMock.Setup(s => s.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync((User?)null);

        var result = await controller.Login(loginRequest);

        Assert.That(result, Is.TypeOf<ViewResult>());
        Assert.That(controller.ModelState.IsValid, Is.False);
    }

    [Test]
    public async Task Login_Post_InvalidPassword_ReturnsViewWithError()
    {
        var controller = CreateController();
        var loginRequest = new LoginRequest { Email = "test@test.com", Password = "wrong" };
        var user = new User
        {
            Id = 1,
            Email = loginRequest.Email,
            Firstname = "joe",
            Lastname = "joe",
            PasswordHash = ""
        };
        
        // Hash un mot de passe différent pour simuler un mauvais mot de passe
        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, "not_the_right_password");

        _userServiceMock.Setup(s => s.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync(user);
        _configurationMock.Setup(c => c["JwtSettings:SecretKey"]).Returns("secret");

        var result = await controller.Login(loginRequest);

        Assert.That(result, Is.TypeOf<ViewResult>());
        Assert.That(controller.ModelState.IsValid, Is.False);
    }
}