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

    public static ArticleDto FromEntity(Article article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            Subtitle = article.Subtitle,
            Content = article.Content,
            MediaPath = article.MediaPath,
            Footer = article.Footer
        };
    }

    public Article ToEntity()
    {
        return new Article
        {
            Id = this.Id,
            Title = this.Title,
            Subtitle = this.Subtitle,
            Content = this.Content,
            MediaPath = this.MediaPath,
            Footer = this.Footer
        };
    }
}