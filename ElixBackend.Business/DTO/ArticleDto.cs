using ElixBackend.Domain.Entities;

namespace ElixBackend.Business.DTO;

public class ArticleDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Content { get; set; }
    public string? MediaPath { get; set; }
    public string? Footer { get; set; }

    // Nouvelle association
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public static ArticleDto FromEntity(Article article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            Subtitle = article.Subtitle,
            Content = article.Content,
            MediaPath = article.MediaPath,
            Footer = article.Footer,
            CategoryId = article.CategoryId,
            Category = article.Category
        };
    }

    public Article ToEntity()
    {
        return new Article
        {
            Id = Id,
            Title = Title,
            Subtitle = Subtitle,
            Content = Content,
            MediaPath = MediaPath,
            Footer = Footer,
            CategoryId = CategoryId,
            Category = Category
        };
    }
}