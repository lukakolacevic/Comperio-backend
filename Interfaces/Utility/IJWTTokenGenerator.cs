using dotInstrukcijeBackend.Interfaces.User;
using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.Interfaces.Utility
{
    public interface IJWTTokenGenerator
    {
        string GenerateJwtToken(IUser user, IConfiguration configuration);
    }

}
