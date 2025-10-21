using ElixBackend.Business.DTO;
using ElixBackend.Domain.Entities;

namespace ElixBackend.Business.IService;

public interface IAnswerService
{
    Task<AnswerDto?> GetByIdAsync(int id);
    Task<IEnumerable<AnswerDto>> GetAllAsync();
    Task AddAsync(AnswerDto answerDto);
    Task UpdateAsync(AnswerDto answerDto);
    Task DeleteAsync(int id);
}