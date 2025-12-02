namespace ElixBackend.Business.DTO;

public class AnswerDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string? Title { get; set; }

    public string? Explanation { get; set; }
    public bool IsValid { get; set; }
    
    public QuestionDto? Question { get; set; }
    
    public static AnswerDto AnswerToAnswerDto(Domain.Entities.Answer answer)
    {
        return new AnswerDto
        {
            Id = answer.Id,
            QuestionId = answer.QuestionId,
            Title = answer.Title,
            Explanation = answer.Explanation,
            IsValid = answer.IsValid
        };
    }
}