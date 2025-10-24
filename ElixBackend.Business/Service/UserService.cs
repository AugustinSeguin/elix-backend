using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;

namespace ElixBackend.Business.Service
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        // Map User entity to UserDto
        private static UserDto? ToDto(User? user)
        {
            if (user == null) return null;
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                Password = user.PasswordHash,
                PasswordRepeated = user.PasswordHash,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Username = user.Username,
                Birthdate = user.BirthDate,
                Gender = user.Gender,
                IsPremium = user.IsPremium,
                PhoneNumber = user.PhoneNumber,
                IsAdmin = user.IsAdmin
            };
        }

        // Use existing converter on DTO to produce User entity when needed

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await userRepository.GetUserByIdAsync(id);
            return ToDto(user);
        }

        public async Task<UserDto?> GetUserByEmailAsync(string? email)
        {
            var user = await userRepository.GetUserByEmailAsync(email);
            return ToDto(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await userRepository.GetAllUsersAsync();
            return users.Select(u => ToDto(u)!).Where(d => d != null)!;
        }

        public async Task<UserDto?> AddUserAsync(UserDto userDto)
        {
            var user = userDto.UserDtoToUser(userDto);
            var newUser = await userRepository.AddUserAsync(user);
            await userRepository.SaveChangesAsync();
            return ToDto(newUser);
        }

        public async Task<UserDto?> UpdateUserAsync(UserDto userDto)
        {
            var user = userDto.UserDtoToUser(userDto);
            var userUpdated = await userRepository.UpdateUserAsync(user);
            await userRepository.SaveChangesAsync();
            return ToDto(userUpdated);
        }

        public async Task DeleteUserAsync(int id)
        {
            await userRepository.DeleteUserAsync(id);
            await userRepository.SaveChangesAsync();
        }
    }
}