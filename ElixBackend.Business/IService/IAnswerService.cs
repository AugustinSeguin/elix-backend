using ElixBackend.Business.DTO;

namespace ElixBackend.Business.IService;

public interface IAnswerService
{
    Task<AnswerDto?> GetByIdAsync(int id);
    Task<IEnumerable<AnswerDto>> GetAllAsync();
    Task AddAsync(AnswerDto answerDto);
    Task UpdateAsync(AnswerDto answerDto);
    Task DeleteAsync(int id);
}