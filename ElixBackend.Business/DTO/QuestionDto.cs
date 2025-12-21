using System.ComponentModel.DataAnnotations;
using ElixBackend.Domain.Enum;

namespace ElixBackend.Business.DTO;

using ElixBackend.Domain.Entities;

public class QuestionDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le titre est requis.")]
    public required string Title { get; set; }

    public string? MediaPath { get; set; }

    public TypeQuestion TypeQuestion { get; set; }

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public IEnumerable<AnswerDto>? Answers { get; set; }

    public static QuestionDto QuestionToQuestionDto(Question question)
    {
        return new QuestionDto
        {
            Id = question.Id,
            Title = question.Title,
            MediaPath = question.MediaPath,
            TypeQuestion = question.TypeQuestion,
            CategoryId = question.CategoryId,
            Answers = question.Answers?.Select(AnswerDto.AnswerToAnswerDto)
        };
    }
}