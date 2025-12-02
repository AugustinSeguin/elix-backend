namespace ElixBackend.Domain.Entities;

public class UserPoint
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int CategoryId { get; set; }

    public int Points { get; set; } = 0;

    public User? User { get; set; }
    public Category? Category { get; set; }
}