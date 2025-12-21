using ElixBackend.Business.DTO;

namespace ElixBackend.Business.IService;

public interface IResourceService
{
    Task<ResourceDto?> AddResourceAsync(ResourceDto resource);
    Task<ResourceDto?> GetResourceByIdAsync(int id);
    Task<IEnumerable<ResourceDto>?> GetAllResourcesAsync();
    Task<ResourceDto?> UpdateResourceAsync(ResourceDto resource);
    Task DeleteResourceAsync(int id);
    Task<IEnumerable<ResourceDto>?> SearchByKeywordAsync(string keyword);
    Task<IEnumerable<ResourceDto>?> SearchByLocalizationAsync(double latitude, double longitude);
}
