using System.Security.Claims;
using System.Threading.Tasks;
using ElixAPI.Middlewares;
using ElixBackend.Business.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace ElixBackend.Tests.Business.Middlewares;

[TestFixture]
public class JwtJtiValidationMiddlewareTest
{
    private Mock<RequestDelegate> _nextMock;
    private Mock<IServiceScopeFactory> _scopeFactoryMock;
    private Mock<IServiceScope> _scopeMock;
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<ITokenService> _tokenServiceMock;

    [SetUp]
    public void SetUp()
    {
        _nextMock = new Mock<RequestDelegate>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _tokenServiceMock = new Mock<ITokenService>();

        _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.SetupGet(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(p => p.GetService(typeof(ITokenService))).Returns(_tokenServiceMock.Object);
    }

    private static ClaimsPrincipal CreatePrincipal(bool authenticated, string? jti = null, string? userId = null)
    {
        var claims = new List<Claim>();
        if (jti != null) claims.Add(new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, jti));
        if (userId != null) claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        var identity = new ClaimsIdentity(claims, authenticated ? "TestAuth" : null);
        return new ClaimsPrincipal(identity);
    }

    [Test]
    public async Task Invoke_NotAuthenticated_CallsNext()
    {
        var context = new DefaultHttpContext();
        context.User = CreatePrincipal(false);

        var middleware = new JwtJtiValidationMiddleware(_nextMock.Object);

        await middleware.Invoke(context, _scopeFactoryMock.Object);

        _nextMock.Verify(n => n.Invoke(context), Times.Once);
    }

    [Test]
    public async Task Invoke_MissingJtiOrUserId_Returns401()
    {
        var context = new DefaultHttpContext();
        context.User = CreatePrincipal(true, jti: null, userId: "1");

        var middleware = new JwtJtiValidationMiddleware(_nextMock.Object);

        await middleware.Invoke(context, _scopeFactoryMock.Object);

        Assert.That(context.Response.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task Invoke_TokenNotExists_Returns401()
    {
        var context = new DefaultHttpContext();
        context.User = CreatePrincipal(true, jti: "jti", userId: "1");

        _tokenServiceMock.Setup(t => t.TokenExistsAsync("jti", 1, It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        var middleware = new JwtJtiValidationMiddleware(_nextMock.Object);

        await middleware.Invoke(context, _scopeFactoryMock.Object);

        Assert.That(context.Response.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task Invoke_TokenExists_CallsNext()
    {
        var context = new DefaultHttpContext();
        context.User = CreatePrincipal(true, jti: "jti", userId: "1");

        _tokenServiceMock.Setup(t => t.TokenExistsAsync("jti", 1, It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        var middleware = new JwtJtiValidationMiddleware(_nextMock.Object);

        await middleware.Invoke(context, _scopeFactoryMock.Object);

        _nextMock.Verify(n => n.Invoke(context), Times.Once);
        Assert.That(context.Response.StatusCode, Is.EqualTo(200)); // Default status
    }
}