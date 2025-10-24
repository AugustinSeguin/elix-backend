using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;

namespace ElixBackend.Business.Service;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<CategoryDto> AddCategoryAsync(CategoryDto category)
    {
        var categoryEntity = new Category
        {
            Title = category.Title,
            Description = category.Description
        };
        var result = await categoryRepository.AddCategoryAsync(categoryEntity);
        await categoryRepository.SaveChangesAsync();
        return CategoryDto.CategoryToCategoryDto(result);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await categoryRepository.GetCategoryByIdAsync(id);
        return category is null ? null : CategoryDto.CategoryToCategoryDto(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await categoryRepository.GetAllCategoriesAsync();
        return categories.Select(CategoryDto.CategoryToCategoryDto);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(CategoryDto category)
    {
        var categoryEntity = new Category
        {
            Id = category.Id,
            Title = category.Title,
            Description = category.Description
        };
        var result = await categoryRepository.UpdateCategoryAsync(categoryEntity);
        await categoryRepository.SaveChangesAsync();
        return CategoryDto.CategoryToCategoryDto(result);
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