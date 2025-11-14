using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ElixBackend.Business.IService;
using ElixBackend.Business.DTO;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ElixBackend.WebApp.Controllers;

[Authorize]
[Route("[controller]")]
public class QuestionController(
    IQuestionService questionService, 
    ICategoryService categoryService, 
    IAnswerService answerService,
    IConfiguration configuration) : Controller
{
    private async Task PopulateCategoriesAsync(int? selectedId = null)
    {
        var categories = await categoryService.GetAllCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Title", selectedId);
    }

    private async Task<string?> HandleMediaUploadAsync(IFormFile? mediaFile, string? existingMediaPath = null)
    {
        if (mediaFile == null || mediaFile.Length == 0)
        {
            return existingMediaPath;
        }

        var uploadsPath = configuration["FileStorage:UploadsPath"] ?? "wwwroot/uploads";
        
        var uploadsFolder = Path.IsPathRooted(uploadsPath) 
            ? uploadsPath 
            : Path.Combine(Directory.GetCurrentDirectory(), uploadsPath);
            
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(mediaFile.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await mediaFile.CopyToAsync(stream);
        }

        if (!string.IsNullOrEmpty(existingMediaPath))
        {
            var oldFileName = Path.GetFileName(existingMediaPath);
            var oldFilePath = Path.IsPathRooted(uploadsPath)
                ? Path.Combine(uploadsPath, oldFileName)
                : Path.Combine(Directory.GetCurrentDirectory(), uploadsPath, oldFileName);
                
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }
        }

        return Path.Combine(uploadsPath, filePath);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Index()
    {
        var questions = await questionService.GetAllQuestionsAsync();
        return View(questions);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Edit(int id)
    {
        var question = await questionService.GetQuestionByIdAsync(id);
        if (question is null) return NotFound();

        await PopulateCategoriesAsync(question.CategoryId);
        
        var answers = await answerService.GetByQuestionIdAsync(id);
        ViewBag.Answers = answers;
        ViewBag.QuestionId = id;
        
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
    public async Task<IActionResult> Create(QuestionDto questionDto, IFormFile? mediaFile)
    {
        ModelState.Remove("MediaPath");
        
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(questionDto.CategoryId);
            return View(questionDto);
        }

        try
        {
            questionDto.MediaPath = await HandleMediaUploadAsync(mediaFile);
            await questionService.AddQuestionAsync(questionDto);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la création: {ex.Message}");
            ModelState.AddModelError(string.Empty, "Impossible de créer la question. Veuillez réessayer.");
            await PopulateCategoriesAsync(questionDto.CategoryId);
            return View(questionDto);
        }
    }

    [HttpPost("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(QuestionDto questionDto, IFormFile? mediaFile)
    {
        ModelState.Remove("MediaPath");
        
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(questionDto.CategoryId);
            var answers = await answerService.GetByQuestionIdAsync(questionDto.Id);
            ViewBag.Answers = answers;
            ViewBag.QuestionId = questionDto.Id;
            return View(questionDto);
        }

        try
        {
            var existingQuestion = await questionService.GetQuestionByIdAsync(questionDto.Id);
            questionDto.MediaPath = await HandleMediaUploadAsync(mediaFile, existingQuestion?.MediaPath);
            
            await questionService.UpdateQuestionAsync(questionDto);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la mise à jour: {ex.Message}");
            ModelState.AddModelError(string.Empty, "Impossible de mettre à jour la question. Veuillez réessayer.");
            await PopulateCategoriesAsync(questionDto.CategoryId);
            var answers = await answerService.GetByQuestionIdAsync(questionDto.Id);
            ViewBag.Answers = answers;
            ViewBag.QuestionId = questionDto.Id;
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
