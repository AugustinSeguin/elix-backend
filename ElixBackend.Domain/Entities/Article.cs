namespace ElixBackend.Domain.Entities;

public class Article
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public string? Subtitle { get; set; }

    public string? Content { get; set; }

    public string? MediaPath { get; set; }

    public string? Footer { get; set; }

    // Nouvelle relation vers Category
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}