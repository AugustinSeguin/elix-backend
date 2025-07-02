using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;

namespace ElixBackend.Business.Service;

public class TokenService(ITokenRepository tokenRepository) : ITokenService
{
    public async Task<UserToken> AddTokenAsync(string jti, int userId)
    {
        return await tokenRepository.AddTokenAsync(jti, userId);
    }

    public async Task RemoveTokenAsync(string jti, int userId)
    {
        await tokenRepository.RemoveTokenAsync(jti, userId);
    }

    public async Task<bool> TokenExistsAsync(string jti, int userId, DateTime dateNow)
    {
        return await tokenRepository.TokenExistsAsync(jti, userId, dateNow);
    }
}