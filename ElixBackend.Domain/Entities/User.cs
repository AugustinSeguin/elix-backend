using ElixBackend.Domain.Helpers;

namespace ElixBackend.Domain.Entities
{
    public class User
    {
        public int? Id { get; set; } 
        public string? Username { get; set; } 
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; } 
        public string PasswordHash { get; set; }
        public DateTime? BirthDate { get; set; } 
        public Gender? Gender { get; set; }
        public bool IsPremium { get; set; } = false;
        
        public ICollection<UserToken> Tokens { get; set; } = new List<UserToken>();
    }
}