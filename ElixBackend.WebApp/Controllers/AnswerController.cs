using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElixBackend.Business.IService;
using ElixBackend.Business.DTO;

namespace ElixBackend.WebApp.Controllers;

[Authorize]
[Route("[controller]")]
public class AnswerController(IAnswerService answerService) : Controller
{
    [HttpGet("[action]")]
    public async Task<IActionResult> Index(int? questionId)
    {
        IEnumerable<AnswerDto> answers = new List<AnswerDto>();

        if (questionId.HasValue)
        {
            answers = await answerService.GetByQuestionIdAsync(questionId.Value);
            ViewBag.QuestionId = questionId.Value;
        }

        return View(answers);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetFormModal(int? id, int? questionId)
    {
        AnswerDto? model;

        if (id is > 0)
        {
            model = await answerService.GetByIdAsync(id.Value);
        }
        else
        {
            model = new AnswerDto
            {
                QuestionId = questionId ?? 0,
                IsValid = false,
                Title = "",
                Explanation = ""
            };
        }

        return PartialView("~/Views/Question/Answer/Form.cshtml", model);
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,QuestionId,Title,Explanation,IsValid")] AnswerDto answerDto)
    {
        ModelState.Remove("Question");
        ModelState.Remove("IsValid"); 
        
        var isValidValue = Request.Form["IsValid"].ToString();
        answerDto.IsValid = isValidValue.Contains("true");
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (answerDto.QuestionId == 0)
        {
            return BadRequest("QuestionId ne peut pas être 0");
        }

        try
        {
            await answerService.AddAsync(answerDto);
            return RedirectToAction("Edit", "Question", new { id = answerDto.QuestionId });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception lors de la création: {ex.Message}");
            ModelState.AddModelError(string.Empty, $"Impossible de créer la réponse: {ex.Message}");
            return BadRequest(ModelState);
        }
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind("Id,QuestionId,Title,Explanation,IsValid")] AnswerDto answerDto)
    {
        ModelState.Remove("Question");
        ModelState.Remove("IsValid");
        
        var isValidValue = Request.Form["IsValid"].ToString();
        answerDto.IsValid = isValidValue.Contains("true");
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await answerService.UpdateAsync(answerDto);
            return RedirectToAction("Edit", "Question", new { id = answerDto.QuestionId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Impossible de modifier la réponse: {ex.Message}");
            return BadRequest(ModelState);
        }
    }

    [HttpPost("Answer/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var answer = await answerService.GetByIdAsync(id);
            if (answer != null)
            {
                var questionId = answer.QuestionId;
                await answerService.DeleteAsync(id);
                return RedirectToAction("Edit", "Question", new { id = questionId });
            }

            return RedirectToAction("Index", "Question");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG Delete - Exception: {ex.Message}");
            return RedirectToAction("Index", "Question");
        }
    }
}