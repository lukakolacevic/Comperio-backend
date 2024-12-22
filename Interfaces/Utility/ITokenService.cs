//using dotInstrukcijeBackend.Interfaces.User;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.ServiceResultUtility;

namespace dotInstrukcijeBackend.Interfaces.Utility
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken(User user);
        Task<ServiceResult<(string, string)>> RefreshToken(string oldRefreshToken);
        string GenerateEmailVerificationToken(int userId);
    }

}
