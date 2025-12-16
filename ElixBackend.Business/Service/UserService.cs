using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.Extensions.Logging;

namespace ElixBackend.Business.Service
{
    public class UserService(IUserRepository userRepository, ILogger<UserService> logger) : IUserService
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
                IsAdmin = user.IsAdmin,
                PictureMediaPath = user.PictureMediaPath
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await userRepository.GetUserByIdAsync(id);
                return ToDto(user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UserService.GetUserByIdAsync failed for id {Id}", id);
                return null;
            }
        }

        public async Task<UserDto?> GetUserByEmailAsync(string? email)
        {
            try
            {
                var user = await userRepository.GetUserByEmailAsync(email);
                return ToDto(user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UserService.GetUserByEmailAsync failed for email {Email}", email);
                return null;
            }
        }

        public async Task<IEnumerable<UserDto>?> GetAllUsersAsync()
        {
            try
            {
                var users = await userRepository.GetAllUsersAsync();
                return users.Select(u => ToDto(u)!).Where(d => d != null)!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UserService.GetAllUsersAsync failed");
                return null;
            }
        }

        public async Task<UserDto?> AddUserAsync(UserDto userDto)
        {
            try
            {
                var user = userDto.UserDtoToUser(userDto);
                var newUser = await userRepository.AddUserAsync(user);
                await userRepository.SaveChangesAsync();
                return ToDto(newUser);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UserService.AddUserAsync failed for {@UserDto}", userDto);
                return null;
            }
        }

        public async Task<UserDto?> UpdateUserAsync(UserDto userDto)
        {
            try
            {
                var user = userDto.UserDtoToUser(userDto);
                var userUpdated = await userRepository.UpdateUserAsync(user);
                await userRepository.SaveChangesAsync();
                return ToDto(userUpdated);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UserService.UpdateUserAsync failed for {@UserDto}", userDto);
                return null;
            }
        }

        public async Task<bool?> DeleteUserAsync(int id)
        {
            try
            {
                await userRepository.DeleteUserAsync(id);
                return await userRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UserService.DeleteUserAsync failed for id {Id}", id);
                return null;
            }
        }
    }
}