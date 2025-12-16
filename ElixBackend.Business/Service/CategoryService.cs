using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.Extensions.Logging;

namespace ElixBackend.Business.Service;

public class CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<CategoryDto?> AddCategoryAsync(CategoryDto category)
    {
        try
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
        catch (Exception ex)
        {
            logger.LogError(ex, "CategoryService.AddCategoryAsync failed for {@Category}", category);
            return null;
        }
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        try
        {
            var category = await categoryRepository.GetCategoryByIdAsync(id);
            return category is null ? null : CategoryDto.CategoryToCategoryDto(category);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CategoryService.GetCategoryByIdAsync failed for id {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<CategoryDto>?> GetAllCategoriesAsync()
    {
        try
        {
            var categories = await categoryRepository.GetAllCategoriesAsync();
            return categories.Select(CategoryDto.CategoryToCategoryDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CategoryService.GetAllCategoriesAsync failed");
            return null;
        }
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(CategoryDto category)
    {
        try
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
        catch (Exception ex)
        {
            logger.LogError(ex, "CategoryService.UpdateCategoryAsync failed for {@Category}", category);
            return null;
        }
    }

    public async Task<bool?> DeleteCategoryAsync(int id)
    {
        try
        {
            await categoryRepository.DeleteCategoryAsync(id);
            var saved = await categoryRepository.SaveChangesAsync();
            return saved;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CategoryService.DeleteCategoryAsync failed for id {Id}", id);
            return null;
        }
    }

    public async Task<bool?> SaveChangesAsync()
    {
        try
        {
            return await categoryRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CategoryService.SaveChangesAsync failed");
            return null;
        }
    }
}