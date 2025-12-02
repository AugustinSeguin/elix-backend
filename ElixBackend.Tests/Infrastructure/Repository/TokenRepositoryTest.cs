using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure;
using ElixBackend.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Tests.Infrastructure.Repository;

[TestFixture]
public class TokenRepositoryTest
{
    private ElixDbContext _context;
    private TokenRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ElixDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ElixDbContext(options);
        _repository = new TokenRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddTokenAsync_AddsTokenAndReturnsIt()
    {
        var jti = "jti-test";
        var userId = 1;

        var token = await _repository.AddTokenAsync(jti, userId);

        Assert.That(token, Is.Not.Null);
        Assert.That(token.Jti, Is.EqualTo(jti));
        Assert.That(token.UserId, Is.EqualTo(userId));
        Assert.That(_context.UserTokens.AnyAsync(t => t.Jti == jti && t.UserId == userId).Result, Is.True);
    }

    [Test]
    public async Task RemoveTokenAsync_RemovesToken()
    {
        var jti = "jti-remove";
        var userId = 2;
        var token = new UserToken { Jti = jti, UserId = userId, ExpirationDate = DateTime.UtcNow.AddHours(1) };
        _context.UserTokens.Add(token);
        await _context.SaveChangesAsync();

        await _repository.RemoveTokenAsync(jti, userId);

        var exists = await _context.UserTokens.AnyAsync(t => t.Jti == jti && t.UserId == userId);
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task TokenExistsAsync_ReturnsTrueIfExistsAndNotExpired()
    {
        var jti = "jti-exists";
        var userId = 3;
        var now = DateTime.UtcNow;
        var token = new UserToken { Jti = jti, UserId = userId, ExpirationDate = now.AddMinutes(10) };
        _context.UserTokens.Add(token);
        await _context.SaveChangesAsync();

        var exists = await _repository.TokenExistsAsync(jti, userId, now);

        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task TokenExistsAsync_ReturnsFalseIfNotExistsOrExpired()
    {
        var jti = "jti-notfound";
        var userId = 4;
        var now = DateTime.UtcNow;

        // Not present
        var exists = await _repository.TokenExistsAsync(jti, userId, now);
        Assert.That(exists, Is.False);

        // Expired
        var expiredToken = new UserToken { Jti = jti, UserId = userId, ExpirationDate = now.AddMinutes(-5) };
        _context.UserTokens.Add(expiredToken);
        await _context.SaveChangesAsync();

        var existsExpired = await _repository.TokenExistsAsync(jti, userId, now);
        Assert.That(existsExpired, Is.False);
    }
}