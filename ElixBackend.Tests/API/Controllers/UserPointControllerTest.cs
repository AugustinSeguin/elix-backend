using System.Security.Claims;
using ElixBackend.API.Controllers;
using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ElixBackend.Tests.API.Controllers;

[TestFixture]
public class UserPointControllerTest
{
    private Mock<IUserPointService> _serviceMock;
    private UserPointController _controller;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IUserPointService>();
        _controller = new UserPointController(_serviceMock.Object);
    }

    private void MockUser(string userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    [Test]
    public async Task GetUserPointsByCategory_ReturnsOk_WhenAuthorizedAndFound()
    {
        int userId = 1;
        int categoryId = 2;
        MockUser(userId.ToString());
        var dto = new UserPointDto { UserId = userId, CategoryId = categoryId, Points = 10 };
        _serviceMock.Setup(s => s.GetUserPointsByCategory(categoryId, userId)).ReturnsAsync(dto);

        var result = await _controller.GetUserPointsByCategory(categoryId, userId);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo(dto));
    }

    [Test]
    public async Task GetUserPointsByCategory_ReturnsForbid_WhenUserIdDoesNotMatch()
    {
        int userId = 1;
        int categoryId = 2;
        MockUser("999"); // Different user ID

        var result = await _controller.GetUserPointsByCategory(categoryId, userId);

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task GetUserPointsByCategory_ReturnsNotFound_WhenServiceReturnsNull()
    {
        int userId = 1;
        int categoryId = 2;
        MockUser(userId.ToString());
        _serviceMock.Setup(s => s.GetUserPointsByCategory(categoryId, userId)).ReturnsAsync((UserPointDto?)null);

        var result = await _controller.GetUserPointsByCategory(categoryId, userId);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task GetUserPoints_ReturnsOk_WhenAuthorizedAndFound()
    {
        int userId = 1;
        MockUser(userId.ToString());
        var list = new List<UserPointDto> { new UserPointDto { UserId = userId, Points = 10 } };
        _serviceMock.Setup(s => s.GetUserPoints(userId)).ReturnsAsync(list);

        var result = await _controller.GetUserPoints(userId);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo(list));
    }

    [Test]
    public async Task GetUserPoints_ReturnsForbid_WhenUserIdDoesNotMatch()
    {
        int userId = 1;
        MockUser("999");

        var result = await _controller.GetUserPoints(userId);

        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task GetUserPoints_ReturnsProblem_WhenServiceReturnsNull()
    {
        int userId = 1;
        MockUser(userId.ToString());
        _serviceMock.Setup(s => s.GetUserPoints(userId)).ReturnsAsync((IEnumerable<UserPointDto>?)null);

        var result = await _controller.GetUserPoints(userId);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }
}
