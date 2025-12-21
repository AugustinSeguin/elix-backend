using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;

namespace ElixBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourceController(IResourceService resourceService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResourceDto>>> GetAll()
    {
        var resources = await resourceService.GetAllResourcesAsync();
        if (resources is null)
            return Problem("Impossible de récupérer les ressources.");

        return Ok(resources);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ResourceDto>> GetById(int id)
    {
        var resource = await resourceService.GetResourceByIdAsync(id);
        if (resource == null)
            return NotFound();

        return Ok(resource);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ResourceDto>> Create(ResourceDto resourceDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdResource = await resourceService.AddResourceAsync(resourceDto);
        if (createdResource == null)
            return Problem("Erreur lors de la création de la ressource.");

        return CreatedAtAction(nameof(GetById), new { id = createdResource.Id }, createdResource);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ResourceDto>> Update(int id, ResourceDto resourceDto)
    {
        if (id != resourceDto.Id)
            return BadRequest("L'ID de l'URL ne correspond pas à l'ID du corps de la requête.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedResource = await resourceService.UpdateResourceAsync(resourceDto);
        if (updatedResource == null)
            return Problem("Erreur lors de la mise à jour de la ressource.");

        return Ok(updatedResource);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        await resourceService.DeleteResourceAsync(id);
        return NoContent();
    }

    [HttpGet("search/keyword")]
    public async Task<ActionResult<IEnumerable<ResourceDto>>> SearchByKeyword([FromQuery] string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest("Le mot-clé est requis.");

        var resources = await resourceService.SearchByKeywordAsync(keyword);
        if (resources is null)
            return Problem("Erreur lors de la recherche.");

        return Ok(resources);
    }

    [HttpGet("search/localization")]
    public async Task<ActionResult<IEnumerable<ResourceDto>>> SearchByLocalization([FromQuery] double latitude, [FromQuery] double longitude)
    {
        var resources = await resourceService.SearchByLocalizationAsync(latitude, longitude);
        if (resources is null)
            return Problem("Erreur lors de la recherche.");

        return Ok(resources);
    }
}
