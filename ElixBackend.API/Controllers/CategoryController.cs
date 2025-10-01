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
        return Ok(CategoryDto.CategoryToCategoryDto(category));
    }

    // POST: api/Category
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<CategoryDto>> Create(CategoryDto dto)
    {
        var created = await categoryService.AddCategoryAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, CategoryDto.CategoryToCategoryDto(created));
    }

    // PUT: api/Category/5
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<CategoryDto>> Update(int id, CategoryDto dto)
    {
        if (id != dto.Id)
            return BadRequest();

        var updated = await categoryService.UpdateCategoryAsync(dto);
        return Ok(CategoryDto.CategoryToCategoryDto(updated));
    }

    // DELETE: api/Category/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await categoryService.DeleteCategoryAsync(id);
        return NoContent();
    }
}