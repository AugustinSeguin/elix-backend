namespace ElixBackend.Business.DTO;

public class AnswerDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string? Title { get; set; }

    public string? Explanation { get; set; }
    public bool IsValid { get; set; }
    
    public QuestionDto? Question { get; set; }
}