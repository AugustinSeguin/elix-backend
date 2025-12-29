using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using System.Security.Claims;

namespace ElixBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserPointController(IUserPointService userPointService) : ControllerBase
{
    [HttpGet("user/{userId}/category/{categoryId}")]
    [Authorize]
    public async Task<ActionResult<UserPointDto>> GetUserPointsByCategory(int categoryId, int userId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null || currentUserId != userId.ToString())
            return Forbid();

        var result = await userPointService.GetUserPointsByCategory(categoryId, userId);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UserPointDto>>> GetUserPoints(int userId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null || currentUserId != userId.ToString())
            return Forbid();

        var result = await userPointService.GetUserPoints(userId);
        if (result == null)
            return Problem("Impossible de récupérer les points.");
        return Ok(result);
    }
}
