namespace ElixBackend.Business.DTO;

public class UserPointDto
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int CategoryId { get; set; }

    public int Points { get; set; } = 0;

    public int MaximumPoints { get; set; } = 0;

    public UserDto? User { get; set; }
    public CategoryDto? Category { get; set; }
}