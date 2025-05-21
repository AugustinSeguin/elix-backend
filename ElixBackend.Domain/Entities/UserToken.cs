namespace ElixBackend.Domain.Entities;

public class UserToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Jti { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }

    public User User { get; set; } = null!;
}