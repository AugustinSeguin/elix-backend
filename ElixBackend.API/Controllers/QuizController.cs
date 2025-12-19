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

    [HttpPost("SubmitQuiz")]
    [Authorize]
    public async Task<ActionResult<List<CorrectionDto>>> SubmitQuiz([FromBody] QuizSubmissionDto quizSubmission)
    {
        if (!quizSubmission.UserAnswers.Any())
        {
            return BadRequest(new { message = "Les données de réponse sont invalides." });
        }

        var result = await quizService.SubmitQuizAsync(quizSubmission);
        
        if (result == null)
        {
            return NotFound(new { message = "Quiz non trouvé." });
        }

        return Ok(result);
    }
}