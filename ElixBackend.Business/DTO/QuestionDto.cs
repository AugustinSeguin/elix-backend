namespace ElixBackend.Business.DTO;

using ElixBackend.Domain.Entities;

public class QuestionDto
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public int CategoryId { get; set; }

    public string? MediaPath { get; set; }

    public static QuestionDto QuestionToQuestionDto(Question question)
    {
        return new QuestionDto
        {
            Id = question.Id,
            Title = question.Title,
            CategoryId = question.CategoryId,
            MediaPath = question.MediaPath
        };
    }
}