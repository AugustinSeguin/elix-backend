using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using Microsoft.AspNetCore.Mvc;

namespace ElixBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnswerController(IAnswerService answerService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AnswerDto>>> GetAll()
    {
        var answers = await answerService.GetAllAsync();
        return Ok(answers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AnswerDto>> GetById(int id)
    {
        var answer = await answerService.GetByIdAsync(id);
        if (answer == null) return NotFound();
        return Ok(answer);
    }

    [HttpPost]
    public async Task<ActionResult> Add([FromBody] AnswerDto dto)
    {
        await answerService.AddAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] AnswerDto dto)
    {
        if (id != dto.Id) return BadRequest();
        await answerService.UpdateAsync(dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await answerService.DeleteAsync(id);
        return NoContent();
    }
}