using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;

namespace ElixBackend.Business.Service;

public class TokenService : ITokenService
{
    private readonly ITokenRepository _tokenRepository;

    public TokenService(ITokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }
    
    public async Task<UserToken> AddTokenAsync(string jti, int userId)
    {
        return await _tokenRepository.AddTokenAsync(jti, userId);
    }
    
    public async Task RemoveTokenAsync(string jti, int userId)
    {
        await _tokenRepository.RemoveTokenAsync(jti, userId);
    }

    public async Task<bool> TokenExistsAsync(string jti, int userId, DateTime dateNow)
    {
        return await _tokenRepository.TokenExistsAsync(jti, userId, dateNow);
    }
}