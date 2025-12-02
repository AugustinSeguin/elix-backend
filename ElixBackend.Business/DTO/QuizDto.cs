namespace ElixBackend.Business.DTO;

public class QuizDto
{
    public int Id { get; set; }

    public string Title { get; set; }

    public int CategoryId { get; set; }

    public CategoryDto? Category { get; set; }

    public List<QuestionDto> Questions { get; set; } = new();
}