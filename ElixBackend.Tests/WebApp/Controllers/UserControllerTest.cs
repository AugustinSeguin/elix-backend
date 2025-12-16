using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.WebApp.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace ElixBackend.Tests.WebApp.Controllers;

[TestFixture]
public class UserControllerTest
{
    private Mock<IUserService> _userServiceMock;
    private Mock<IConfiguration> _configurationMock;
    private Mock<ITokenService> _tokenServiceMock;
    private UserController _controller;

    [SetUp]
    public void SetUp()
    {
        _userServiceMock = new Mock<IUserService>();
        _configurationMock = new Mock<IConfiguration>();
        _tokenServiceMock = new Mock<ITokenService>();

        _controller = new UserController(_userServiceMock.Object, _configurationMock.Object, _tokenServiceMock.Object);

        // Setup TempData and HttpContext
        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    #region Login GET Tests

    [Test]
    public void Login_Get_ReturnsView()
    {
        // Act
        var result = _controller.Login() as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    #endregion

    #region Login POST Tests

    [Test]
    public async Task Login_Post_WithNullUser_ReturnsViewWithError()
    {
        // Arrange
        var loginRequest = new LoginRequestDto { Email = "notfound@test.com", Password = "password" };
        _userServiceMock.Setup(s => s.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.Login(loginRequest) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
        Assert.That(_controller.ModelState[""]?.Errors[0].ErrorMessage, Is.EqualTo("Email ou mot de passe invalide."));
    }

    [Test]
    public async Task Login_Post_WithWrongPassword_ReturnsViewWithError()
    {
        // Arrange
        var loginRequest = new LoginRequestDto { Email = "test@test.com", Password = "wrongpassword" };
        var passwordHasher = new PasswordHasher<UserDto>();
        var userDto = new UserDto
        {
            Id = 1,
            Email = "test@test.com",
            Username = "testuser",
            Firstname = "Test",
            Lastname = "User",
            Password = passwordHasher.HashPassword(null, "correctpassword"),
            PasswordRepeated = ""
        };
        _userServiceMock.Setup(s => s.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync(userDto);

        // Act
        var result = await _controller.Login(loginRequest) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
    }

    #endregion

    #region Index Tests

    [Test]
    public async Task Index_ReturnsViewWithAllUsers()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new() { Id = 1, Username = "user1", Email = "user1@test.com", Firstname = "John", Lastname = "Doe", Password = "hash1", PasswordRepeated = "hash1" },
            new() { Id = 2, Username = "user2", Email = "user2@test.com", Firstname = "Jane", Lastname = "Smith", Password = "hash2", PasswordRepeated = "hash2" }
        };
        _userServiceMock.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);

        // Act
        var result = await _controller.Index() as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<IEnumerable<UserDto>>());
        var model = result.Model as IEnumerable<UserDto>;
        Assert.That(model.Count(), Is.EqualTo(2));
        _userServiceMock.Verify(s => s.GetAllUsersAsync(), Times.Once);
    }

    [Test]
    public async Task Index_WithEmptyList_ReturnsViewWithEmptyList()
    {
        // Arrange
        var users = new List<UserDto>();
        _userServiceMock.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);

        // Act
        var result = await _controller.Index() as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        var model = result.Model as IEnumerable<UserDto>;
        Assert.That(model.Count(), Is.EqualTo(0));
    }

    #endregion

    #region Create GET Tests

    [Test]
    public void Create_Get_ReturnsViewWithNewUser()
    {
        // Act
        var result = _controller.Create() as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<UserDto>());
        var model = result.Model as UserDto;
        Assert.That(model.Username, Is.EqualTo(""));
        Assert.That(model.Email, Is.EqualTo(""));
    }

    #endregion

    #region Create POST Tests

    [Test]
    public async Task Create_Post_WithValidModel_RedirectsToIndex()
    {
        // Arrange
        var userDto = new UserDto
        {
            Username = "newuser",
            Email = "new@test.com",
            Password = "password123",
            PasswordRepeated = "password123",
            Firstname = "New",
            Lastname = "User"
        };
        
        // Mock Request.Form pour les checkboxes
        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "IsAdmin", "false" },
            { "IsPremium", "false" }
        });
        _controller.ControllerContext.HttpContext.Request.ContentType = "application/x-www-form-urlencoded";
        _controller.ControllerContext.HttpContext.Request.Form = formCollection;
        
        _userServiceMock.Setup(s => s.AddUserAsync(It.IsAny<UserDto>()))
            .ReturnsAsync(new UserDto { Id = 1, Username = "newuser", Email = "new@test.com", Firstname = "New", Lastname = "User", Password = "hashed", PasswordRepeated = "hashed" });

        // Act
        var result = await _controller.Create(userDto) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _userServiceMock.Verify(s => s.AddUserAsync(It.IsAny<UserDto>()), Times.Once);
    }

    [Test]
    public async Task Create_Post_WithInvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var userDto = new UserDto
        {
            Username = "",
            Email = "new@test.com",
            Password = "password123",
            PasswordRepeated = "password123",
            Firstname = "New",
            Lastname = "User"
        };
        
        // Mock Request.Form pour les checkboxes
        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "IsAdmin", "false" },
            { "IsPremium", "false" }
        });
        _controller.ControllerContext.HttpContext.Request.ContentType = "application/x-www-form-urlencoded";
        _controller.ControllerContext.HttpContext.Request.Form = formCollection;
        
        _controller.ModelState.AddModelError("Username", "Le nom d'utilisateur est requis");

        // Act
        var result = await _controller.Create(userDto) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<UserDto>());
        _userServiceMock.Verify(s => s.AddUserAsync(It.IsAny<UserDto>()), Times.Never);
    }

    [Test]
    public async Task Create_Post_ServiceThrowsException_ReturnsViewWithError()
    {
        // Arrange
        var userDto = new UserDto
        {
            Username = "newuser",
            Email = "new@test.com",
            Password = "password123",
            PasswordRepeated = "password123",
            Firstname = "New",
            Lastname = "User"
        };
        
        // Mock Request.Form pour les checkboxes
        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "IsAdmin", "false" },
            { "IsPremium", "false" }
        });
        _controller.ControllerContext.HttpContext.Request.ContentType = "application/x-www-form-urlencoded";
        _controller.ControllerContext.HttpContext.Request.Form = formCollection;
        
        _userServiceMock.Setup(s => s.AddUserAsync(It.IsAny<UserDto>())).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Create(userDto) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<UserDto>());
        Assert.That(_controller.ModelState.ErrorCount, Is.GreaterThan(0));
    }

    #endregion

    #region Edit GET Tests

    [Test]
    public async Task Edit_Get_WithValidId_ReturnsViewWithUser()
    {
        // Arrange
        var userId = 5;
        var user = new UserDto
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            Firstname = "Test",
            Lastname = "User",
            Password = "hashed",
            PasswordRepeated = "hashed"
        };
        _userServiceMock.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _controller.Edit(userId) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<UserDto>());
        var model = result.Model as UserDto;
        Assert.That(model.Id, Is.EqualTo(userId));
        Assert.That(model.Username, Is.EqualTo("testuser"));
        _userServiceMock.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
    }

    [Test]
    public async Task Edit_Get_WithNullUser_ReturnsNotFound()
    {
        // Arrange
        var userId = 999;
        _userServiceMock.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.Edit(userId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    #endregion

    #region Edit POST Tests

    [Test]
    public async Task Edit_Post_WithEmptyPassword_KeepsOldPassword()
    {
        // Arrange
        var userDto = new UserDto
        {
            Id = 5,
            Username = "updateduser",
            Email = "updated@test.com",
            Password = "",
            PasswordRepeated = "",
            Firstname = "Updated",
            Lastname = "User"
        };
        var existingUser = new UserDto
        {
            Id = 5,
            Username = "olduser",
            Email = "old@test.com",
            Password = "old_hashed_password",
            PasswordRepeated = "old_hashed_password",
            Firstname = "Old",
            Lastname = "User"
        };

        // Mock Request.Form pour les checkboxes
        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "IsAdmin", "false" },
            { "IsPremium", "false" }
        });
        _controller.ControllerContext.HttpContext.Request.ContentType = "application/x-www-form-urlencoded";
        _controller.ControllerContext.HttpContext.Request.Form = formCollection;

        _userServiceMock.Setup(s => s.GetUserByIdAsync(5)).ReturnsAsync(existingUser);
        _userServiceMock.Setup(s => s.UpdateUserAsync(It.IsAny<UserDto>()))
            .ReturnsAsync(new UserDto { Id = 5, Username = "updateduser", Email = "updated@test.com", Firstname = "Updated", Lastname = "User", Password = "old_hashed_password", PasswordRepeated = "old_hashed_password" });

        // Act
        var result = await _controller.Edit(userDto) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _userServiceMock.Verify(s => s.UpdateUserAsync(It.Is<UserDto>(u => u.Password == "old_hashed_password")), Times.Once);
    }

    [Test]
    public async Task Edit_Post_ServiceThrowsException_ReturnsViewWithError()
    {
        // Arrange
        var userDto = new UserDto
        {
            Id = 5,
            Username = "updateduser",
            Email = "updated@test.com",
            Password = "",
            PasswordRepeated = "",
            Firstname = "Updated",
            Lastname = "User"
        };
        var existingUser = new UserDto
        {
            Id = 5,
            Username = "olduser",
            Email = "old@test.com",
            Password = "old_hashed_password",
            PasswordRepeated = "old_hashed_password",
            Firstname = "Old",
            Lastname = "User"
        };

        // Mock Request.Form pour les checkboxes
        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "IsAdmin", "false" },
            { "IsPremium", "false" }
        });
        _controller.ControllerContext.HttpContext.Request.ContentType = "application/x-www-form-urlencoded";
        _controller.ControllerContext.HttpContext.Request.Form = formCollection;

        _userServiceMock.Setup(s => s.GetUserByIdAsync(5)).ReturnsAsync(existingUser);
        _userServiceMock.Setup(s => s.UpdateUserAsync(It.IsAny<UserDto>())).ThrowsAsync(new Exception("Update failed"));

        // Act
        var result = await _controller.Edit(userDto) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Model, Is.InstanceOf<UserDto>());
        Assert.That(_controller.ModelState.ErrorCount, Is.GreaterThan(0));
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task Delete_WithValidId_RedirectsToIndex()
    {
        // Arrange
        var userId = 5;
        _userServiceMock.Setup(s => s.DeleteUserAsync(userId)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(userId) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _userServiceMock.Verify(s => s.DeleteUserAsync(userId), Times.Once);
    }

    [Test]
    public async Task Delete_ServiceThrowsException_RedirectsToIndex()
    {
        // Arrange
        var userId = 5;
        _userServiceMock.Setup(s => s.DeleteUserAsync(userId)).ThrowsAsync(new Exception("Delete failed"));

        // Act
        var result = await _controller.Delete(userId) as RedirectToActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Index"));
        _userServiceMock.Verify(s => s.DeleteUserAsync(userId), Times.Once);
    }

    #endregion
}
