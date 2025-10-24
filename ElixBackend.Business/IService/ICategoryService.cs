using ElixBackend.Business.DTO;
using ElixBackend.Domain.Entities;

namespace ElixBackend.Business.IService;

public interface ICategoryService
{
    Task<CategoryDto> AddCategoryAsync(CategoryDto category);
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto> UpdateCategoryAsync(CategoryDto category);
    Task DeleteCategoryAsync(int id);
    Task<bool> SaveChangesAsync();
}