using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.ServiceResultUtility;
using dotInstrukcijeBackend.ViewModels;

namespace dotInstrukcijeBackend.Interfaces.Service
{
    public interface IUserService
    {
        Task<ServiceResult<User>> FindUserByEmailAsync(int roleId, string email);
        Task<ServiceResult<IEnumerable<User>>> FindAllUsersByRoleIdAsync(int roleId);
        Task<ServiceResult> ConfirmEmailAsync(int id);
        Task<ServiceResult> RegisterUserAsync(int roleId, RegistrationModel model);
        Task<ServiceResult<(User, string, string)>> LoginUserAsync(int roleId, LoginModel model);
        Task<ServiceResult<User>> FindUserByIdAsync(int id);
        Task<ServiceResult<User>> RegisterGoogleUserAsync(int roleId, User googleUser);
    }
}
