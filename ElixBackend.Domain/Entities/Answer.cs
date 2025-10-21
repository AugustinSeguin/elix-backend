namespace ElixBackend.Domain.Entities;

public class Answer
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string? Title { get; set; }
    public bool IsValid { get; set; } = false;
    public string? Explanation { get; set; }

    public Question? Question { get; set; }
}