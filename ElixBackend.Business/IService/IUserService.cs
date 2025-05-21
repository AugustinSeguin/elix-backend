using ElixBackend.Business.DTO;
using ElixBackend.Domain.Entities;
 
 namespace ElixBackend.Business.IService
 {
     public interface IUserService
     {
         Task<User> GetUserByIdAsync(int id);
         Task<User> GetUserByEmailAsync(string email);
         Task<IEnumerable<User>> GetAllUsersAsync();
         Task<User>  AddUserAsync(UserDTO user);
         Task<User>  UpdateUserAsync(UserDTO user);
         Task DeleteUserAsync(int id);
     }
 }