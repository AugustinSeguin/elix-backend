using ElixBackend.Domain.Entities;

namespace ElixBackend.Infrastructure.IRepository;

public interface IUserAnswerRepository
{
    Task<UserAnswer> AddUserAnswerAsync(UserAnswer userAnswer);

    Task<IEnumerable<UserAnswer?>> GetUserAnswerByUserIdAsync(int userId, int questionId);

    Task<UserAnswer> UpdateUserAnswerAsync(UserAnswer userAnswer);

    Task DeleteUserAnswerAsync(int id);

    Task<bool> SaveChangesAsync();
}