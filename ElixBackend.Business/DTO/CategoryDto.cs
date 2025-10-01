namespace ElixBackend.Business.DTO;

using ElixBackend.Domain.Entities;

public class CategoryDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }

    public static CategoryDto CategoryToCategoryDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Title = category.Title,
            Description = category.Description
        };
    }
}