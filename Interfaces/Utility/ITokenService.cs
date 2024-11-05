using dotInstrukcijeBackend.Interfaces.User;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.ServiceResultUtility;

namespace dotInstrukcijeBackend.Interfaces.Utility
{
    public interface ITokenService
    {
        string GenerateAccessToken(IUser user);
        string GenerateRefreshToken(IUser user);
        Task<ServiceResult<(string, string)>> RefreshToken(string oldRefreshToken);
    }

}
