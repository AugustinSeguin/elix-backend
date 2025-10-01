using ElixBackend.Business.DTO;
using ElixBackend.Domain.Entities;

namespace ElixBackend.Business.IService;

public interface ICategoryService
{
    Task<Category> AddCategoryAsync(CategoryDto category);
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<Category> UpdateCategoryAsync(CategoryDto category);
    Task DeleteCategoryAsync(int id);
    Task<bool> SaveChangesAsync();
}