using System.ComponentModel.DataAnnotations;

namespace ElixBackend.Business.DTO;

using ElixBackend.Domain.Entities;

public class QuestionDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le titre est requis.")]
    public required string Title { get; set; }

    public string? MediaPath { get; set; }
    
    public int CategoryId { get; set; }
    
    public Category? Category { get; set; }

    public static QuestionDto QuestionToQuestionDto(Question question)
    {
        return new QuestionDto
        {
            Id = question.Id,
            Title = question.Title,
            MediaPath = question.MediaPath,
            CategoryId = question.CategoryId
        };
    }
}