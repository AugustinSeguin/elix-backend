using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using ElixBackend.Infrastructure.IRepository;

namespace ElixBackend.Business.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<User> AddUserAsync(UserDTO userDto)
        {
            var user = userDto.UserDtoToUser(userDto);
            var newUser = await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();
            return newUser;
        }

        public async Task<User> UpdateUserAsync(UserDTO userDto)
        {
            var user = userDto.UserDtoToUser(userDto);
            var userUpdated = await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();
            return userUpdated;
        }

        public async Task DeleteUserAsync(int id)
        {
            await _userRepository.DeleteUserAsync(id);
            await _userRepository.SaveChangesAsync();
        }
    }
}