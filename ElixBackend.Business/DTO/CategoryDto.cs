namespace ElixBackend.Business.DTO;

using ElixBackend.Domain.Entities;
using System.ComponentModel.DataAnnotations;

public class CategoryDto
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Le titre est requis.")]
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? ImageMediaPath { get; set; }

    public static CategoryDto CategoryToCategoryDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Title = category.Title,
            Description = category.Description,
            ImageMediaPath = category.ImageMediaPath
        };
    }
}