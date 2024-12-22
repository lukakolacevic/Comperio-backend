using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.Interfaces.Repository
{
    public interface IUserRepository
    {
        Task<int> AddUserAsync(User user);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task SetEmailVerifiedAsync(int id);
        Task<User> GetUserByEmailAsync(int roleId, string email);
        Task<User> GetUserByIdAsync(int id);
        Task<bool> IsUserVerifiedAsync(int id);
        Task<IEnumerable<User>> GetAllUsersByRoleAsync(int roleId);
    }
}
