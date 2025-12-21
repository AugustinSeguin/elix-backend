using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ElixBackend.Business.IService;
using ElixBackend.Business.DTO;
using ElixBackend.Business.Helpers;
using ElixBackend.Domain.Enum;

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

    private void PopulateQuestionTypes(TypeQuestion? selectedType = null)
    {
        var types = Enum.GetValues(typeof(TypeQuestion))
            .Cast<TypeQuestion>()
            .Select(t => new SelectListItem
            {
                Value = t.ToString(),
                Text = t switch
                {
                    TypeQuestion.QuizModeMcq => "QCM",
                    TypeQuestion.TrueFalseActive => "Vrai ou faux",
                    _ => t.ToString()
                }
            });

        ViewBag.TypeQuestions = new SelectList(types, "Value", "Text", selectedType);
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
        PopulateQuestionTypes(question.TypeQuestion);

        var answers = await answerService.GetByQuestionIdAsync(id);
        ViewBag.Answers = answers;
        ViewBag.QuestionId = id;

        return View(question);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Create()
    {
        PopulateQuestionTypes();
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
            PopulateQuestionTypes(questionDto.TypeQuestion);
            await PopulateCategoriesAsync(questionDto.CategoryId);
            return View(questionDto);
        }

        try
        {
            questionDto.MediaPath = await MediaHelper.HandleMediaUploadAsync(mediaFile, configuration);
            await questionService.AddQuestionAsync(questionDto);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la création: {ex.Message}");
            ModelState.AddModelError(string.Empty, "Impossible de créer la question. Veuillez réessayer.");
            PopulateQuestionTypes(questionDto.TypeQuestion);
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
            PopulateQuestionTypes(questionDto.TypeQuestion);
            await PopulateCategoriesAsync(questionDto.CategoryId);
            var answers = await answerService.GetByQuestionIdAsync(questionDto.Id);
            ViewBag.Answers = answers;
            ViewBag.QuestionId = questionDto.Id;
            return View(questionDto);
        }

        try
        {
            var existingQuestion = await questionService.GetQuestionByIdAsync(questionDto.Id);
            questionDto.MediaPath = await MediaHelper.HandleMediaUploadAsync(mediaFile, configuration, existingQuestion?.MediaPath);

            await questionService.UpdateQuestionAsync(questionDto);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la mise à jour: {ex.Message}");
            ModelState.AddModelError(string.Empty, "Impossible de mettre à jour la question. Veuillez réessayer.");
            await PopulateCategoriesAsync(questionDto.CategoryId);
            PopulateQuestionTypes(questionDto.TypeQuestion);
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
