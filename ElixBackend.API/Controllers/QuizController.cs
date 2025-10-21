using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using Microsoft.AspNetCore.Mvc;

namespace ElixBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuizDto>>> GetAll()
    {
        var quizzes = await _quizService.GetAllQuizzesAsync();
        return Ok(quizzes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QuizDto>> GetById(int id)
    {
        var quiz = await _quizService.GetQuizByIdAsync(id);
        if (quiz == null) return NotFound();
        return Ok(quiz);
    }

    [HttpPost]
    public async Task<ActionResult<QuizDto>> Create([FromBody] QuizDto quizDto)
    {
        var created = await _quizService.AddQuizAsync(quizDto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<QuizDto>> Update(int id, [FromBody] QuizDto quizDto)
    {
        if (id != quizDto.Id) return BadRequest();
        var updated = await _quizService.UpdateQuizAsync(quizDto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _quizService.DeleteQuizAsync(id);
        return NoContent();
    }
}