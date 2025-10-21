namespace ElixBackend.Domain.Entities;

public class Quiz
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}