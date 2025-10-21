using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;

namespace ElixBackend.Business.Service;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _quizRepository;

    public QuizService(IQuizRepository quizRepository)
    {
        _quizRepository = quizRepository;
    }

    public async Task<QuizDto?> GetQuizByIdAsync(int id)
    {
        var quiz = await _quizRepository.GetQuizByIdAsync(id);
        return quiz == null ? null : QuizToQuizDto(quiz);
    }

    public async Task<IEnumerable<QuizDto>> GetAllQuizzesAsync()
    {
        var quizzes = await _quizRepository.GetAllQuizzesAsync();
        return quizzes.Select(QuizToQuizDto);
    }

    public async Task<QuizDto> AddQuizAsync(QuizDto quizDto)
    {
        var quiz = new Quiz
        {
            Title = quizDto.Title,
            CategoryId = quizDto.CategoryId
        };
        var created = await _quizRepository.AddQuizAsync(quiz);
        await _quizRepository.SaveChangesAsync();
        return QuizToQuizDto(created);
    }

    public async Task<QuizDto> UpdateQuizAsync(QuizDto quizDto)
    {
        var quiz = new Quiz
        {
            Id = quizDto.Id,
            Title = quizDto.Title,
            CategoryId = quizDto.CategoryId
        };
        var updated = await _quizRepository.UpdateQuizAsync(quiz);
        await _quizRepository.SaveChangesAsync();
        return QuizToQuizDto(updated);
    }

    public async Task DeleteQuizAsync(int id)
    {
        await _quizRepository.DeleteQuizAsync(id);
        await _quizRepository.SaveChangesAsync();
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _quizRepository.SaveChangesAsync();
    }

    private QuizDto QuizToQuizDto(Quiz quiz)
    {
        return new QuizDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            CategoryId = quiz.CategoryId,
            Category = quiz.Category
        };
    }
}