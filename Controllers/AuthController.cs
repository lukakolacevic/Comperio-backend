using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Interfaces.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace dotInstrukcijeBackend.Controllers
{
    
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
       
        public AuthController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var oldRefreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(oldRefreshToken))
            {
                return Unauthorized(new { message = "Refresh token not found" });
            }

            var result = await _tokenService.RefreshToken(oldRefreshToken);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            var (newAccessToken, newRefreshToken) = result.Data;

            Response.Cookies.Append("accessToken", newAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,                 
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15) 
            });

            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,                 
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7) 
            });

            return Ok(new
            {
                success = true,
                message = "Token refreshed successfully."
            });
        }
    }
}
