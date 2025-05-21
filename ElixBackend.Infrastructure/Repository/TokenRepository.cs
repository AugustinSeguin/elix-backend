using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Infrastructure.Repository;

public class TokenRepository : ITokenRepository
{
    private readonly ElixDbContext _context;

    public TokenRepository(ElixDbContext context)
    {
        _context = context;
    }

    public async Task<UserToken> AddTokenAsync(string jti, int userId)
    {
        var userToken = new UserToken
        {
            UserId = userId,
            Jti = jti,
            ExpirationDate = DateTime.UtcNow.AddHours(2)
        };
        await _context.UserTokens.AddAsync(userToken);
        await _context.SaveChangesAsync();
        return userToken;
    }

    public async Task RemoveTokenAsync(string jti, int userId)
    {
        var token = await _context.UserTokens
            .FirstOrDefaultAsync(t => t.Jti == jti && t.UserId == userId);

        if (token != null)
        {
            _context.UserTokens.Remove(token);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> TokenExistsAsync(string jti, int userId, DateTime dateNow)
    {
        return await _context.UserTokens.AnyAsync(t => t.Jti == jti && t.UserId == userId && t.ExpirationDate >= dateNow);
    }
}