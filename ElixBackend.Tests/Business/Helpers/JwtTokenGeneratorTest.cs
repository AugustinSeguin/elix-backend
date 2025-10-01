using ElixBackend.API.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ElixBackend.Tests.Business.Helpers;

[TestFixture]
public class JwtTokenGeneratorTest
{
    private const string SecretKey = "0123456789abcdef0123456789abcdef";

    [Test]
    public void GenerateToken_Returns_Valid_JWT()
    {
        // Arrange
        var userId = "42";

        // Act
        var token = JwtTokenGenerator.GenerateToken(userId, SecretKey, out var jti);

        // Assert
        Assert.That(token, Is.Not.Null.And.Not.Empty);
        Assert.That(jti, Is.Not.Null.And.Not.Empty);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.That(jwt.Claims, Has.Some.Matches<Claim>(c => c.Type == "nameid" && c.Value == userId.ToString()));
        Assert.That(jwt.ValidTo, Is.GreaterThan(DateTime.UtcNow));
    }

    [Test]
    public void GenerateToken_Throws_If_SecretKey_NullOrEmpty()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            JwtTokenGenerator.GenerateToken("42", null, out _);
        });
        Assert.Throws<ArgumentNullException>(() =>
        {
            JwtTokenGenerator.GenerateToken("42", "", out _);
        });
    }

    [Test]
    public void GenerateAdminToken_Returns_Valid_JWT_With_AdminRole()
    {
        // Arrange
        var userId = 99;

        // Act
        var token = JwtTokenGenerator.GenerateAdminToken(userId, SecretKey, out var jti);

        // Assert
        Assert.That(token, Is.Not.Null.And.Not.Empty);
        Assert.That(jti, Is.Not.Null.And.Not.Empty);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

       Assert.That(jwt.Claims, Has.Some.Matches<Claim>(c => c.Type == "nameid" && c.Value == userId.ToString()));
        Assert.That(jwt.Claims, Has.Some.Matches<Claim>(c => c.Type == JwtRegisteredClaimNames.Jti && c.Value == jti));
        Assert.That(jwt.ValidTo, Is.GreaterThan(DateTime.UtcNow));
    }

    [Test]
    public void GenerateAdminToken_Throws_If_SecretKey_NullOrEmpty()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            JwtTokenGenerator.GenerateAdminToken(99, null, out _);
        });
        Assert.Throws<ArgumentNullException>(() =>
        {
            JwtTokenGenerator.GenerateAdminToken(99, "", out _);
        });
    }
}