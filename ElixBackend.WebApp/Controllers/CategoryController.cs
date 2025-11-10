using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElixBackend.Business.IService;
using ElixBackend.Business.DTO;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ElixBackend.WebApp.Controllers;

[Authorize]
[Route("[controller]")]
public class CategoryController(ICategoryService categoryService) : Controller
{
    [HttpGet("[action]")]
    public async Task<IActionResult> Index()
    {
        var categories = await categoryService.GetAllCategoriesAsync();
        return View(categories);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await categoryService.GetCategoryByIdAsync(id);

        return View(category);
    }

    [HttpGet("[action]")]
    public IActionResult Create()
    {
        return View(new CategoryDto
        {
            Title = ""
        });
    }

    // POST: /Category/Create
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryDto categoryDto)
    {
        if (!ModelState.IsValid)
        {
            return View(categoryDto);
        }

        try
        {
            await categoryService.AddCategoryAsync(categoryDto);
            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Impossible de créer la catégorie. Veuillez réessayer.");
            return View(categoryDto);
        }
    }

    [HttpPost("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryDto categoryDto)
    {
        if (!ModelState.IsValid)
        {
            return View(categoryDto);
        }

        try
        {
            await categoryService.UpdateCategoryAsync(categoryDto);
            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Impossible de mettre à jour la catégorie. Veuillez réessayer.");
            return View(categoryDto);
        }
    }

    [HttpPost("Category/Delete/{id:int}")]
    [ActionName("Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await categoryService.DeleteCategoryAsync(id);
            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            return RedirectToAction("Index");
        }
    }
}