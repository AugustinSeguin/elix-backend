using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Infrastructure.Repository;

public class TokenRepository(ElixDbContext context) : ITokenRepository
{
    public async Task<UserToken> AddTokenAsync(string jti, int userId)
    {
        var userToken = new UserToken
        {
            UserId = userId,
            Jti = jti,
            ExpirationDate = DateTime.UtcNow.AddHours(2)
        };
        await context.UserTokens.AddAsync(userToken);
        await context.SaveChangesAsync();
        return userToken;
    }

    public async Task RemoveTokenAsync(string jti, int userId)
    {
        var token = await context.UserTokens
            .FirstOrDefaultAsync(t => t.Jti == jti && t.UserId == userId);

        if (token != null)
        {
            context.UserTokens.Remove(token);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> TokenExistsAsync(string jti, int userId, DateTime dateNow)
    {
        return await context.UserTokens.AnyAsync(t =>
            t.Jti == jti && t.UserId == userId && t.ExpirationDate >= dateNow);
    }
}