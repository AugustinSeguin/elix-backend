using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Domain.Enum;
using ElixBackend.Infrastructure.IRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ElixBackend.Business.Service
{
    public class UserService(IUserRepository userRepository, IUserPointService userPointService, ILogger<UserService> logger, IConfiguration configuration) : IUserService
    {
        private readonly string _apiBaseUrl = (configuration["ApiBaseUrl"] ?? string.Empty).TrimEnd('/');
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

        private static UserDto? Sanitize(UserDto? user)
        {
            if (user == null) return null;
            user.Password = string.Empty;
            user.PasswordRepeated = string.Empty;
            return user;
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await userRepository.GetUserByIdAsync(id);
                return Sanitize(ToDto(user));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UserService.GetUserByIdAsync failed for id {Id}", id);
                return null;
            }
        }

        public async Task<UserDto?> GetUserByEmailAsync(string? email, bool includePassword = false)
        {
            try
            {
                var user = await userRepository.GetUserByEmailAsync(email);
                var dto = ToDto(user);
                return includePassword ? dto : Sanitize(dto);
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
                return users.Select(ToDto).Select(Sanitize).Where(d => d != null).Select(d => d!);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UserService.GetAllUsersAsync failed");
                return null;
            }
        }

        public async Task<UserDto?> AddUserAsync(UserDto user)
        {
            try
            {
                var entity = user.UserDtoToUser(user);
                var newUser = await userRepository.AddUserAsync(entity);
                await userRepository.SaveChangesAsync();
                return Sanitize(ToDto(newUser));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UserService.AddUserAsync failed for {@UserDto}", user);
                return null;
            }
        }

        public async Task<UserDto?> UpdateUserAsync(UserDto user)
        {
            try
            {
                var entity = user.UserDtoToUser(user);
                var userUpdated = await userRepository.UpdateUserAsync(entity);
                await userRepository.SaveChangesAsync();
                return Sanitize(ToDto(userUpdated));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UserService.UpdateUserAsync failed for {@UserDto}", user);
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

        public async Task<UserDto?> GetMeAsync(int userId)
        {
            try
            {
                var user = await userRepository.GetUserByIdAsync(userId);
                var dto = ToDto(user);
                if (dto == null)
                {
                    return null;
                }

                var pointsList = await userPointService.GetUserPoints(userId);
                dto.UserPoints = pointsList?.ToList() ?? new List<UserPointDto>();

                var totalPoints = await userPointService.GetTotalPointsByUserIdAsync(userId) ?? 0;
                dto.BadgeUrl = GetBadgeUrl(totalPoints);

                return Sanitize(dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "UserService.GetMeAsync failed for userId {UserId}", userId);
                return null;
            }
        }

        private string GetBadgeUrl(int totalPoints)
        {
            var fileName = GetBadgeFileName(totalPoints);
            if (string.IsNullOrWhiteSpace(_apiBaseUrl))
            {
                return $"/{fileName}";
            }

            return $"{_apiBaseUrl}/{fileName}";
        }

        private static string GetBadgeFileName(int totalPoints)
        {
            if (totalPoints >= (int)Points.Advanced)
            {
                return "advanced.png";
            }
            if (totalPoints >= (int)Points.Confirmed)
            {
                return "confirmed.png";
            }
            return "beginner.png";
        }
    }
}