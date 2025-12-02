using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElixBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController(IQuizService quizService) : ControllerBase
{
    [HttpGet("StartQuiz")]
    [Authorize]
    public async Task<ActionResult<QuizDto>> StartQuiz([FromQuery] int userId, [FromQuery] int categoryId)
    {
        var quiz = await quizService.StartQuizAsync(userId, categoryId);
        
        if (quiz == null)
        {
            return NotFound(new { message = "Aucune quiz disponible pour cette catégorie." });
        }

        return Ok(quiz);
    }
}