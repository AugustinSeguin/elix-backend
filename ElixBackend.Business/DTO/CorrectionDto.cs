namespace ElixBackend.Business.DTO;

public class CorrectionDto
{
    public int QuestionId { get; set; }
    public QuestionDto Question { get; set; }
    public int SelectedAnswerId { get; set; }
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }
    public AnswerDto Answer { get; set; }
}