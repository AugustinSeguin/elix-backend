using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.Extensions.Logging;

namespace ElixBackend.Business.Service;

public class ResourceService(IResourceRepository resourceRepository, ILogger<ResourceService> logger) : IResourceService
{
    public async Task<ResourceDto?> AddResourceAsync(ResourceDto resourceDto)
    {
        try
        {
            var resourceEntity = new Resource
            {
                Name = resourceDto.Name,
                Localization = new Localization
                {
                    Latitude = resourceDto.Localization.Latitude,
                    Longitude = resourceDto.Localization.Longitude
                },
                PhoneNumber = resourceDto.PhoneNumber
            };
            var result = await resourceRepository.AddResourceAsync(resourceEntity);
            await resourceRepository.SaveChangesAsync();
            return ResourceDto.ResourceToResourceDto(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ResourceService.AddResourceAsync failed for {@Resource}", resourceDto);
            return null;
        }
    }

    public async Task<ResourceDto?> GetResourceByIdAsync(int id)
    {
        try
        {
            var resource = await resourceRepository.GetResourceByIdAsync(id);
            return resource is null ? null : ResourceDto.ResourceToResourceDto(resource);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ResourceService.GetResourceByIdAsync failed for id {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<ResourceDto>?> GetAllResourcesAsync()
    {
        try
        {
            var resources = await resourceRepository.GetAllResourcesAsync();
            return resources.Select(ResourceDto.ResourceToResourceDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ResourceService.GetAllResourcesAsync failed");
            return null;
        }
    }

    public async Task<ResourceDto?> UpdateResourceAsync(ResourceDto resourceDto)
    {
        try
        {
            var resourceEntity = new Resource
            {
                Id = resourceDto.Id,
                Name = resourceDto.Name,
                Localization = new Localization
                {
                    Latitude = resourceDto.Localization.Latitude,
                    Longitude = resourceDto.Localization.Longitude
                },
                PhoneNumber = resourceDto.PhoneNumber
            };
            var result = await resourceRepository.UpdateResourceAsync(resourceEntity);
            await resourceRepository.SaveChangesAsync();
            return ResourceDto.ResourceToResourceDto(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ResourceService.UpdateResourceAsync failed for {@Resource}", resourceDto);
            return null;
        }
    }

    public async Task DeleteResourceAsync(int id)
    {
        try
        {
            await resourceRepository.DeleteResourceAsync(id);
            await resourceRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ResourceService.DeleteResourceAsync failed for id {Id}", id);
        }
    }

    public async Task<IEnumerable<ResourceDto>?> SearchByKeywordAsync(string keyword)
    {
        try
        {
            var resources = await resourceRepository.SearchByKeywordAsync(keyword);
            return resources.Select(ResourceDto.ResourceToResourceDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ResourceService.SearchByKeywordAsync failed for keyword {Keyword}", keyword);
            return null;
        }
    }

    public async Task<IEnumerable<ResourceDto>?> SearchByLocalizationAsync(double latitude, double longitude)
    {
        try
        {
            var resources = await resourceRepository.SearchByLocalizationAsync(latitude, longitude);
            return resources.Select(ResourceDto.ResourceToResourceDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ResourceService.SearchByLocalizationAsync failed for lat {Latitude} long {Longitude}", latitude, longitude);
            return null;
        }
    }
}
