using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;

namespace ElixBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(ICategoryService categoryService) : ControllerBase
{
    // GET: api/Category
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var categories = await categoryService.GetAllCategoriesAsync();
        if (categories is null)
            return Problem("Impossible de récupérer les catégories.");
        return Ok(categories);
    }

    // GET: api/Category/5
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var category = await categoryService.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound();
        return Ok(category);
    }

    // POST: api/Category
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<CategoryDto>> Create(CategoryDto dto)
    {
        var created = await categoryService.AddCategoryAsync(dto);
        if (created is null)
            return Problem("La création de la catégorie a échoué.");
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/Category/5
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<CategoryDto>> Update(int id, CategoryDto dto)
    {
        if (id != dto.Id)
            return BadRequest();

        var updated = await categoryService.UpdateCategoryAsync(dto);
        if (updated is null)
            return Problem("La mise à jour de la catégorie a échoué.");
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
}