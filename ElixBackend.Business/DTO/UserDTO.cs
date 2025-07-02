using ElixBackend.Domain.Entities;
using ElixBackend.Domain.Helpers;

namespace ElixBackend.Business.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordRepeated { get; set; }
        public string Firstname { get; set; }

        public string Lastname { get; set; }
        public string? Username { get; set; }
        public DateTime? Birthdate { get; set; }
        public Gender? Gender { get; set; }
        public bool IsPremium { get; set; } = false;

        public User UserDtoToUser(UserDTO userDto)
        {
            return new User
            {
                Id = userDto.Id,
                Email = userDto.Email,
                BirthDate = userDto.Birthdate,
                Firstname = userDto.Firstname,
                Username = userDto.Username,
                Lastname = userDto.Lastname,
                Gender = userDto.Gender,
                IsPremium = userDto.IsPremium,
                PasswordHash = userDto.Password
            };
        }

    }
}