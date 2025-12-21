using ElixBackend.Domain.Entities;

namespace ElixBackend.Infrastructure.IRepository;

public interface IResourceRepository
{
    Task<Resource> AddResourceAsync(Resource resource);
    Task<Resource?> GetResourceByIdAsync(int id);
    Task<IEnumerable<Resource>> GetAllResourcesAsync();
    Task<Resource> UpdateResourceAsync(Resource resource);
    Task DeleteResourceAsync(int id);
    Task<IEnumerable<Resource>> SearchByKeywordAsync(string keyword);
    Task<IEnumerable<Resource>> SearchByLocalizationAsync(double latitude, double longitude);
    Task<bool> SaveChangesAsync();
}
