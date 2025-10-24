using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Infrastructure.Repository;

public class QuizRepository(ElixDbContext context) : IQuizRepository
{

    public async Task<Quiz?> GetQuizByIdAsync(int id)
    {
        // Utiliser AsNoTracking pour éviter les problèmes de tracking dans les tests
        return await context.Quizzes
            .AsNoTracking()
            .Include(q => q.Category)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<IEnumerable<Quiz>> GetAllQuizzesAsync()
    {
        // Récupérer sans tracking pour éviter conflits si les entités sont manipulées ailleurs
        return await context.Quizzes
            .AsNoTracking()
            .Include(q => q.Category)
            .ToListAsync();
    }

    public async Task<Quiz> AddQuizAsync(Quiz quiz)
    {
        var entry = await context.Quizzes.AddAsync(quiz);
        return entry.Entity;
    }

    public async Task<Quiz> UpdateQuizAsync(Quiz quiz)
    {
        // Préférer mettre à jour l'entité déjà trackée pour éviter les conflits de tracking
        var existing = await context.Quizzes.FindAsync(quiz.Id);
        if (existing != null)
        {
            existing.Title = quiz.Title;
            existing.CategoryId = quiz.CategoryId;
            existing.Category = quiz.Category;
            return existing;
        }

        var entry = context.Quizzes.Update(quiz);
        return entry.Entity;
    }

    public async Task DeleteQuizAsync(int id)
    {
        var quiz = await context.Quizzes.FindAsync(id);
        if (quiz != null)
        {
            context.Quizzes.Remove(quiz);
        }
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}