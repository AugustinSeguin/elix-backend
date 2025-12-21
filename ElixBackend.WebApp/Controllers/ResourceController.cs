using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElixBackend.Business.IService;
using ElixBackend.Business.DTO;

namespace ElixBackend.WebApp.Controllers;

[Authorize]
[Route("[controller]")]
public class ResourceController(IResourceService resourceService) : Controller
{
    [HttpGet("[action]")]
    public async Task<IActionResult> Index()
    {
        var resources = await resourceService.GetAllResourcesAsync();
        return View(resources);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Edit(int id)
    {
        var resource = await resourceService.GetResourceByIdAsync(id);
        return View(resource);
    }

    [HttpGet("[action]")]
    public IActionResult Create()
    {
        return View(new ResourceDto
        {
            Name = "",
            Localization = new LocalizationDto()
        });
    }

    // POST: /Resource/Create
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResourceDto resourceDto)
    {
        if (!ModelState.IsValid)
        {
            return View(resourceDto);
        }

        try
        {
            await resourceService.AddResourceAsync(resourceDto);
            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Impossible de créer la ressource. Veuillez réessayer.");
            return View(resourceDto);
        }
    }

    [HttpPost("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ResourceDto resourceDto)
    {
        if (!ModelState.IsValid)
        {
            return View(resourceDto);
        }

        try
        {
            await resourceService.UpdateResourceAsync(resourceDto);
            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Impossible de mettre à jour la ressource. Veuillez réessayer.");
            return View(resourceDto);
        }
    }

    [HttpPost("Resource/Delete/{id:int}")]
    [ActionName("Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await resourceService.DeleteResourceAsync(id);
            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            return RedirectToAction("Index");
        }
    }
}
