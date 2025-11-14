// File: `ElixBackend.WebApp/Controllers/QuestionController.cs`
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ElixBackend.Business.IService;
using ElixBackend.Business.DTO;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ElixBackend.WebApp.Controllers;

[Authorize]
[Route("[controller]")]
public class QuestionController(IQuestionService questionService, ICategoryService categoryService) : Controller
{
    private async Task PopulateCategoriesAsync(int? selectedId = null)
    {
        var categories = await categoryService.GetAllCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Title", selectedId);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Index()
    {
        var categories = await questionService.GetAllQuestionsAsync();
        return View(categories);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Edit(int id)
    {
        var question = await questionService.GetQuestionByIdAsync(id);
        if (question is null) return NotFound();

        await PopulateCategoriesAsync(question.CategoryId);
        return View(question);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesAsync();
        return View(new QuestionDto
        {
            Title = ""
        });
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuestionDto questionDto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(questionDto.CategoryId);
            return View(questionDto);
        }

        try
        {
            await questionService.AddQuestionAsync(questionDto);
            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Impossible de créer la question. Veuillez réessayer.");
            await PopulateCategoriesAsync(questionDto.CategoryId);
            return View(questionDto);
        }
    }

    [HttpPost("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(QuestionDto questionDto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(questionDto.CategoryId);
            return View(questionDto);
        }

        try
        {
            await questionService.UpdateQuestionAsync(questionDto);
            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Impossible de mettre à jour la question. Veuillez réessayer.");
            await PopulateCategoriesAsync(questionDto.CategoryId);
            return View(questionDto);
        }
    }

    [HttpPost("Question/Delete/{id:int}")]
    [ActionName("Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await questionService.DeleteQuestionAsync(id);
            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            return RedirectToAction("Index");
        }
    }
}
