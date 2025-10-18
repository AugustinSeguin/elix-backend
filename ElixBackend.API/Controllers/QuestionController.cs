using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;

namespace ElixBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionController(IQuestionService questionService) : ControllerBase
{
    // GET: api/Question
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<QuestionDto>>> GetAll()
    {
        var questions = await questionService.GetAllQuestionsAsync();
        return Ok(questions);
    }

    // GET: api/Question/5
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<QuestionDto>> GetById(int id)
    {
        var question = await questionService.GetQuestionByIdAsync(id);
        if (question == null)
            return NotFound();
        return Ok(question);
    }

    // POST: api/Question
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<QuestionDto>> Create(QuestionDto dto)
    {
        var created = await questionService.AddQuestionAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/Question/5
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<QuestionDto>> Update(int id, QuestionDto dto)
    {
        if (id != dto.Id)
            return BadRequest();

        var updated = await questionService.UpdateQuestionAsync(dto);
        return Ok(updated);
    }

    // DELETE: api/Question/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await questionService.DeleteQuestionAsync(id);
        return NoContent();
    }
}