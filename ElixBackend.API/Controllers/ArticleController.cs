using Microsoft.AspNetCore.Mvc;
using ElixBackend.Business.IService;
using ElixBackend.Business.DTO;
using Microsoft.AspNetCore.Authorization;

namespace ElixBackend.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ArticleController(IArticleService articleService, IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllArticlesAsync()
    {
        var articles = await articleService.GetAllArticlesAsync();
        if (articles == null) return NotFound();
        
        // Convertir les chemins d'images en URL complètes
        var articlesList = articles.ToList();
        foreach (var article in articlesList)
        {
            if (!string.IsNullOrWhiteSpace(article.MediaPath))
            {
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var fileName = Path.GetFileName(article.MediaPath);
                article.MediaPath = $"{baseUrl}/uploads/{fileName}";
            }
        }
        
        return Ok(articlesList);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetArticleByIdAsync(int id)
    {
        var article = await articleService.GetArticleByIdAsync(id);
        if (article == null) return NotFound();
        
        // Convertir le chemin de l'image en URL complète
        if (!string.IsNullOrWhiteSpace(article.MediaPath))
        {
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var fileName = Path.GetFileName(article.MediaPath);
            article.MediaPath = $"{baseUrl}/uploads/{fileName}";
        }
        
        return Ok(article);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetArticlesByCategoryAsync(int categoryId)
    {
        var articles = await articleService.GetArticlesByCategoryAsync(categoryId);
        if (articles == null) return NotFound();
        
        // Convertir les chemins d'images en URL complètes
        var articlesList = articles.ToList();
        foreach (var article in articlesList)
        {
            if (!string.IsNullOrWhiteSpace(article.MediaPath))
            {
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var fileName = Path.GetFileName(article.MediaPath);
                article.MediaPath = $"{baseUrl}/uploads/{fileName}";
            }
        }
        
        return Ok(articlesList);
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

    // GET: /article/{articleId}/{fileName}
    [HttpGet("/article/{articleId}/{fileName}")]
    public IActionResult GetArticleImage(int articleId, string fileName)
    {
        try
        {
            var uploadsPath = configuration["FileStorage:UploadsPath"] ?? "wwwroot/uploads";
            var uploadsFolder = Path.IsPathRooted(uploadsPath) 
                ? uploadsPath 
                : Path.Combine(Directory.GetCurrentDirectory(), uploadsPath);
            
            var filePath = Path.Combine(uploadsFolder, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Image non trouvée.");
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            var imageBytes = System.IO.File.ReadAllBytes(filePath);
            return File(imageBytes, contentType);
        }
        catch (Exception)
        {
            return NotFound("Erreur lors de la récupération de l'image.");
        }
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestArticlesAsync()
    {
        var articles = await articleService.GetLatestArticlesAsync(2);
        if (articles == null) return NotFound();

        // Convertir les chemins d'images en URL complètes
        var articlesList = articles.ToList();
        foreach (var article in articlesList)
        {
            if (!string.IsNullOrWhiteSpace(article.MediaPath))
            {
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var fileName = Path.GetFileName(article.MediaPath);
                article.MediaPath = $"{baseUrl}/uploads/{fileName}";
            }
        }

        return Ok(articlesList);
    }
}