namespace ElixBackend.Business.DTO;

public class QuizSubmissionDto
{
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public List<UserAnsweredQuestionDto> UserAnswers { get; set; } = new();
}

public class UserAnsweredQuestionDto
{
    public int QuestionId { get; set; }
    public int AnswerIdSelected { get; set; }
}

