using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;

namespace ElixBackend.Business.Service
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await userRepository.GetUserByIdAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await userRepository.GetUserByEmailAsync(email);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await userRepository.GetAllUsersAsync();
        }

        public async Task<User?> AddUserAsync(UserDTO userDto)
        {
            var user = userDto.UserDtoToUser(userDto);
            var newUser = await userRepository.AddUserAsync(user);
            await userRepository.SaveChangesAsync();
            return newUser;
        }

        public async Task<User?> UpdateUserAsync(UserDTO userDto)
        {
            var user = userDto.UserDtoToUser(userDto);
            var userUpdated = await userRepository.UpdateUserAsync(user);
            await userRepository.SaveChangesAsync();
            return userUpdated;
        }

        public async Task DeleteUserAsync(int id)
        {
            await userRepository.DeleteUserAsync(id);
            await userRepository.SaveChangesAsync();
        }
    }
}