using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ElixBackend.Business.IService;
using ElixBackend.Business.DTO;
using ElixBackend.Business.Helpers;

namespace ElixBackend.WebApp.Controllers;

[Authorize]
[Route("[controller]")]
public class ArticleController(
    IArticleService articleService, 
    ICategoryService categoryService,
    IConfiguration configuration) : Controller
{
    private async Task PopulateCategoriesAsync(int? selectedId = null)
    {
        var categories = await categoryService.GetAllCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Title", selectedId);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Index()
    {
        var articles = await articleService.GetAllArticlesAsync();
        return View(articles);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Edit(int id)
    {
        var article = await articleService.GetArticleByIdAsync(id);
        if (article is null) return NotFound();

        await PopulateCategoriesAsync(article.CategoryId);
        return View(article);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesAsync();
        return View(new ArticleDto
        {
            Title = ""
        });
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ArticleDto articleDto, IFormFile? mediaFile)
    {
        ModelState.Remove("MediaPath");
        
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(articleDto.CategoryId);
            return View(articleDto);
        }

        try
        {
            articleDto.MediaPath = await MediaHelper.HandleMediaUploadAsync(mediaFile, configuration);
            await articleService.AddArticleAsync(articleDto);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la création: {ex.Message}");
            ModelState.AddModelError(string.Empty, "Impossible de créer l'article. Veuillez réessayer.");
            await PopulateCategoriesAsync(articleDto.CategoryId);
            return View(articleDto);
        }
    }

    [HttpPost("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ArticleDto articleDto, IFormFile? mediaFile)
    {
        ModelState.Remove("MediaPath");
        
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(articleDto.CategoryId);
            return View(articleDto);
        }

        try
        {
            var existingArticle = await articleService.GetArticleByIdAsync(articleDto.Id);
            articleDto.MediaPath = await MediaHelper.HandleMediaUploadAsync(mediaFile, configuration, existingArticle?.MediaPath);
            
            await articleService.UpdateArticleAsync(articleDto);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la mise à jour: {ex.Message}");
            ModelState.AddModelError(string.Empty, "Impossible de mettre à jour l'article. Veuillez réessayer.");
            await PopulateCategoriesAsync(articleDto.CategoryId);
            return View(articleDto);
        }
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await articleService.DeleteArticleAsync(id);
            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            return RedirectToAction("Index");
        }
    }
}