using ElixBackend.Domain.Entities;

namespace ElixBackend.Business.DTO;

public class UserAnswerDto
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int AnswerId { get; set; }

    public bool IsCorrect { get; set; } = false;

    public UserDto? User { get; set; }
    public AnswerDto? Answer { get; set; }
    
    public static UserAnswerDto QuestionToQuestionDto(UserAnswer ua)
    {
        return new UserAnswerDto
        {
            Id = ua.Id,
            UserId = ua.UserId,
            AnswerId = ua.AnswerId,
            IsCorrect = ua.IsCorrect
        };
    }
}