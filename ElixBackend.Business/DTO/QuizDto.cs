using ElixBackend.Domain.Entities;

namespace ElixBackend.Business.DTO;

public class QuizDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}