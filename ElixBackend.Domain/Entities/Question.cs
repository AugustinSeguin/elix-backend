namespace ElixBackend.Domain.Entities;

public class Question
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public string? MediaPath { get; set; }
    
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}