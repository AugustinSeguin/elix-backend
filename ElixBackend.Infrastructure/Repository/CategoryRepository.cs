using System.Collections.Generic;
using System.Threading.Tasks;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Infrastructure.Repository;

public class CategoryRepository(ElixDbContext context) : ICategoryRepository
{
    public async Task<Category> AddCategoryAsync(Category category)
    {
        var entry = await context.Categories.AddAsync(category);
        return entry.Entity;
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await context.Categories.FindAsync(id);
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await context.Categories.ToListAsync();
    }

    public Task<Category> UpdateCategoryAsync(Category category)
    {
        var local = context.Categories.Local.FirstOrDefault(c => c.Id == category.Id);
        if (local != null)
        {
            context.Entry(local).CurrentValues.SetValues(category);
            return Task.FromResult(local);
        }

        var entry = context.Categories.Update(category);
        return Task.FromResult(entry.Entity);
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category != null)
        {
            context.Categories.Remove(category);
        }
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}