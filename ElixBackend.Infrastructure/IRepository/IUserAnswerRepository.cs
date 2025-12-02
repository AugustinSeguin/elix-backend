using ElixBackend.Domain.Entities;

namespace ElixBackend.Infrastructure.IRepository;

public interface IUserAnswerRepository
{
    Task<UserAnswer> AddUserAnswerAsync(UserAnswer userAnswer);

    Task<UserAnswer?> GetUserAnswerByIdAsync(int id);

    Task<IEnumerable<UserAnswer>> GetAllUserAnswersAsync();

    Task<UserAnswer> UpdateUserAnswerAsync(UserAnswer userAnswer);

    Task DeleteUserAnswerAsync(int id);

    Task<bool> SaveChangesAsync();
}