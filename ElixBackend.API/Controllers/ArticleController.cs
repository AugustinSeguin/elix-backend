using Microsoft.AspNetCore.Mvc;
using ElixBackend.Business.IService;
using ElixBackend.Business.DTO;

namespace ElixBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticleController(IArticleService articleService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllArticlesAsync()
    {
        var articles = await articleService.GetAllArticlesAsync();
        return Ok(articles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetArticleByIdAsync(int id)
    {
        var article = await articleService.GetArticleByIdAsync(id);
        if (article == null) return NotFound();
        return Ok(article);
    }

    [HttpPost]
    public async Task<IActionResult> AddArticleAsync([FromBody] ArticleDto articleDto)
    {
        var created = await articleService.AddArticleAsync(articleDto);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateArticleAsync(int id, [FromBody] ArticleDto articleDto)
    {
        if (id != articleDto.Id) return BadRequest("L'ID dans l'URL doit correspondre à l'ID de l'article.");
        var updated = await articleService.UpdateArticleAsync(articleDto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArticleAsync(int id)
    {
        await articleService.DeleteArticleAsync(id);
        return NoContent();
    }
}