using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ElixBackend.Infrastructure.Repository;

public class ResourceRepository(ElixDbContext context) : IResourceRepository
{
    public async Task<Resource> AddResourceAsync(Resource resource)
    {
        var entry = await context.Resources.AddAsync(resource);
        return entry.Entity;
    }

    public async Task<Resource?> GetResourceByIdAsync(int id)
    {
        return await context.Resources.FindAsync(id);
    }

    public async Task<IEnumerable<Resource>> GetAllResourcesAsync()
    {
        return await context.Resources.ToListAsync();
    }

    public Task<Resource> UpdateResourceAsync(Resource resource)
    {
        var local = context.Resources.Local.FirstOrDefault(r => r.Id == resource.Id);
        if (local != null)
        {
            context.Entry(local).CurrentValues.SetValues(resource);
            // Handle owned entity update if necessary, but SetValues might handle it if properties match.
            // For owned types, we might need to update them explicitly if they are not replaced.
            if (resource.Localization != null)
            {
                local.Localization.Latitude = resource.Localization.Latitude;
                local.Localization.Longitude = resource.Localization.Longitude;
            }
            return Task.FromResult(local);
        }

        var entry = context.Resources.Update(resource);
        return Task.FromResult(entry.Entity);
    }

    public async Task DeleteResourceAsync(int id)
    {
        var resource = await context.Resources.FindAsync(id);
        if (resource != null)
        {
            context.Resources.Remove(resource);
        }
    }

    public async Task<IEnumerable<Resource>> SearchByKeywordAsync(string keyword)
    {
        return await context.Resources
            .Where(r => r.Name.Contains(keyword))
            .ToListAsync();
    }

    public async Task<IEnumerable<Resource>> SearchByLocalizationAsync(double latitude, double longitude)
    {
        // Simple implementation: find resources within ~10km (approx 0.1 degree)
        var range = 0.1;
        return await context.Resources
            .Where(r => r.Localization.Latitude >= latitude - range && r.Localization.Latitude <= latitude + range &&
                        r.Localization.Longitude >= longitude - range && r.Localization.Longitude <= longitude + range)
            .ToListAsync();
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
