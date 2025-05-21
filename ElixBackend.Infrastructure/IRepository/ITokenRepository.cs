using ElixBackend.Domain.Entities;

namespace ElixBackend.Infrastructure.IRepository;

public interface ITokenRepository
{
    Task<UserToken> AddTokenAsync(string jti, int userId);

    Task RemoveTokenAsync(string jti, int userId);
    
    Task<bool> TokenExistsAsync(string jti, int userId, DateTime dateNow);
}