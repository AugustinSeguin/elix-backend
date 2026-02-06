using ElixBackend.Domain.Entities;
using ElixBackend.Domain.Enum;

namespace ElixBackend.Business.DTO
{
    public class UserDto
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string PasswordRepeated { get; set; }
        public required string Firstname { get; set; }

        public required string Lastname { get; set; }
        public string? Username { get; set; }
        public DateTime? Birthdate { get; set; }
        public Gender? Gender { get; set; }
        public bool IsPremium { get; set; } = false;
        public int? PhoneNumber { get; set; }
        public bool IsAdmin { get; set; } = false;

        public string? PictureMediaPath { get; set; }

        public List<UserPointDto>? UserPoints { get; set; }

        public string? BadgeUrl { get; set; }


        public User UserDtoToUser(UserDto userDto)
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
                PasswordHash = userDto.Password,
                PhoneNumber = userDto.PhoneNumber,
                IsAdmin = userDto.IsAdmin,
                PictureMediaPath = userDto.PictureMediaPath
            };
        }
    }
}