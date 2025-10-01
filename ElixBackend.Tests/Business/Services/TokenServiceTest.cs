using System;
using System.Threading.Tasks;
using ElixBackend.Business.Service;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Moq;
using NUnit.Framework;

namespace ElixBackend.Tests.Business.Services;

[TestFixture]
public class TokenServiceTest
{
    private Mock<ITokenRepository> _tokenRepositoryMock;
    private TokenService _tokenService;

    [SetUp]
    public void SetUp()
    {
        _tokenRepositoryMock = new Mock<ITokenRepository>();
        _tokenService = new TokenService(_tokenRepositoryMock.Object);
    }

    [Test]
    public async Task AddTokenAsync_CallsRepositoryAndReturnsToken()
    {
        var expectedToken = new UserToken { Id = 1, UserId = 2, Jti = "jti", ExpirationDate = DateTime.UtcNow.AddHours(1) };
        _tokenRepositoryMock.Setup(r => r.AddTokenAsync("jti", 2)).ReturnsAsync(expectedToken);

        var result = await _tokenService.AddTokenAsync("jti", 2);

        Assert.That(result, Is.EqualTo(expectedToken));
        _tokenRepositoryMock.Verify(r => r.AddTokenAsync("jti", 2), Times.Once);
    }

    [Test]
    public async Task RemoveTokenAsync_CallsRepository()
    {
        _tokenRepositoryMock.Setup(r => r.RemoveTokenAsync("jti", 2)).Returns(Task.CompletedTask);

        await _tokenService.RemoveTokenAsync("jti", 2);

        _tokenRepositoryMock.Verify(r => r.RemoveTokenAsync("jti", 2), Times.Once);
    }

    [Test]
    public async Task TokenExistsAsync_CallsRepositoryAndReturnsResult()
    {
        _tokenRepositoryMock.Setup(r => r.TokenExistsAsync("jti", 2, It.IsAny<DateTime>())).ReturnsAsync(true);

        var result = await _tokenService.TokenExistsAsync("jti", 2, DateTime.UtcNow);

        Assert.That(result, Is.True);
        _tokenRepositoryMock.Verify(r => r.TokenExistsAsync("jti", 2, It.IsAny<DateTime>()), Times.Once);
    }
}