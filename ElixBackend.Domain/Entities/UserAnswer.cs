namespace ElixBackend.Domain.Entities;

public class UserAnswer
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int QuestionId { get; set; }

    public bool IsCorrect { get; set; } = false;

    public User? User { get; set; }
    public Question? Question { get; set; }
}