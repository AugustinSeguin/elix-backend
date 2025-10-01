using ElixBackend.Domain.Entities;

namespace ElixBackend.Infrastructure.IRepository;

public interface ICategoryRepository
{
    Task<Category> AddCategoryAsync(Category category);
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Category> UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(int id);
    Task<bool> SaveChangesAsync();
}