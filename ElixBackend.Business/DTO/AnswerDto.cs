namespace ElixBackend.Business.DTO;

public class AnswerDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Title { get; set; }

    public string? Explanation { get; set; }
    public bool IsValid { get; set; }

    public static AnswerDto AnswerToAnswerDto(ElixBackend.Domain.Entities.Answer answer)
    {
        return new AnswerDto
        {
            Id = answer.Id,
            QuestionId = answer.QuestionId,
            Title = answer.Title,
            IsValid = answer.IsValid,
            Explanation = answer.Explanation
        };
    }
}