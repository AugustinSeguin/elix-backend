using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Business.Helpers;

namespace ElixBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(ICategoryService categoryService, IConfiguration configuration) : ControllerBase
{
    // GET: api/Category
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var categories = await categoryService.GetAllCategoriesAsync();
        if (categories is null)
            return Problem("Impossible de récupérer les catégories.");
        
        // Convertir les chemins d'images en URL complètes
        var categoriesList = categories.ToList();
        foreach (var category in categoriesList)
        {
            if (!string.IsNullOrWhiteSpace(category.ImageMediaPath))
            {
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var fileName = Path.GetFileName(category.ImageMediaPath);
                category.ImageMediaPath = $"{baseUrl}/uploads/{fileName}";
            }
        }
        
        return Ok(categoriesList);
    }

    // GET: api/Category/5
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var category = await categoryService.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound();
        
        // Convertir le chemin de l'image en URL complète
        if (!string.IsNullOrWhiteSpace(category.ImageMediaPath))
        {
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var fileName = Path.GetFileName(category.ImageMediaPath);
            category.ImageMediaPath = $"{baseUrl}/uploads/{fileName}";
        }
        
        return Ok(category);
    }

    // POST: api/Category
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryDto dto)
    {
        var created = await categoryService.AddCategoryAsync(dto);
        if (created is null)
            return Problem("La création de la catégorie a échoué.");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/Category/5
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] CategoryDto dto)
    {
        if (id != dto.Id)
            return BadRequest();

        var existingCategory = await categoryService.GetCategoryByIdAsync(id);
        if (existingCategory == null)
            return NotFound();

        // Conserver l'image existante si non fournie
        if (string.IsNullOrWhiteSpace(dto.ImageMediaPath))
        {
            dto.ImageMediaPath = existingCategory.ImageMediaPath;
        }

        var updated = await categoryService.UpdateCategoryAsync(dto);
        if (updated is null)
            return Problem("La mise à jour de la catégorie a échoué.");
        return Ok(updated);
    }

    // PUT: api/Category/{id}/image
    [HttpPut("{id}/image")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<CategoryDto>> UpdateCategoryImage(int id, IFormFile imageFile)
    {
        var existingCategory = await categoryService.GetCategoryByIdAsync(id);
        if (existingCategory == null)
            return NotFound();

        if (imageFile is not { Length: > 0 })
            return BadRequest("Aucun fichier fourni.");

        var savedPath = await MediaHelper.HandleMediaUploadAsync(imageFile, configuration, existingCategory.ImageMediaPath);
        existingCategory.ImageMediaPath = savedPath;

        var updated = await categoryService.UpdateCategoryAsync(existingCategory);

        if (updated != null && !string.IsNullOrWhiteSpace(updated.ImageMediaPath))
        {
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var fileName = Path.GetFileName(updated.ImageMediaPath);
            updated.ImageMediaPath = $"{baseUrl}/uploads/{fileName}";
        }

        return Ok(updated);
    }

    // DELETE: api/Category/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await categoryService.DeleteCategoryAsync(id);
        if (deleted is null)
            return Problem("La suppression de la catégorie a échoué.");
        return NoContent();
    }

    // GET: /category/{categoryId}/{fileName}
    [HttpGet("/category/{categoryId}/{fileName}")]
    public IActionResult GetCategoryImage(int categoryId, string fileName)
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
}