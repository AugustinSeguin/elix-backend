using ElixBackend.Domain.Entities;

namespace ElixBackend.Business.DTO;

public class UserAnswerDto
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int QuestionId { get; set; }

    public bool IsCorrect { get; set; } = false;

    public UserDto? User { get; set; }
    public Question? Question { get; set; }

    public int QuestionAnswered { get; set; } = 0;
    
    public static UserAnswerDto UserAnswerToUserAnswerDto(UserAnswer ua)
    {
        return new UserAnswerDto
        {
            Id = ua.Id,
            UserId = ua.UserId,
            QuestionId = ua.QuestionId,
            IsCorrect = ua.IsCorrect
        };
    }
}