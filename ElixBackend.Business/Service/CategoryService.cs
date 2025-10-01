using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;

namespace ElixBackend.Business.Service;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<Category> AddCategoryAsync(CategoryDto categoryDto)
    {
        var category = new Category
        {
            Title = categoryDto.Title,
            Description = categoryDto.Description
        };
        var result = await categoryRepository.AddCategoryAsync(category);
        await categoryRepository.SaveChangesAsync();
        return result;
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await categoryRepository.GetCategoryByIdAsync(id);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await categoryRepository.GetAllCategoriesAsync();
        return categories.Select(CategoryDto.CategoryToCategoryDto);
    }

    public async Task<Category> UpdateCategoryAsync(CategoryDto categoryDto)
    {
        var category = new Category
        {
            Id = categoryDto.Id,
            Title = categoryDto.Title,
            Description = categoryDto.Description
        };
        var result = await categoryRepository.UpdateCategoryAsync(category);
        await categoryRepository.SaveChangesAsync();
        return result;
    }

    public async Task DeleteCategoryAsync(int id)
    {
        await categoryRepository.DeleteCategoryAsync(id);
        await categoryRepository.SaveChangesAsync();
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await categoryRepository.SaveChangesAsync();
    }
}