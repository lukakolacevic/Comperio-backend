using Azure.Core;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Interfaces.Service;
using dotInstrukcijeBackend.Interfaces.ServiceInterfaces;
using dotInstrukcijeBackend.Interfaces.Utility;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace dotInstrukcijeBackend.Controllers
{
    
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IProfessorService _professorService;
        private readonly IStudentService _studentService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenService _tokenService;
       
        public AuthController(ITokenService tokenService, IProfessorService professorService, IStudentService studentService, IHttpContextAccessor httpContextAccessor)
        {
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
            _professorService = professorService;
            _studentService = studentService;
        }

        [HttpPost("register/{user}")]
        public async Task<IActionResult> Register(string user, [FromForm] RegistrationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            if (user == "student")
            {
                var result = await _studentService.RegisterStudentAsync(model);

                if (!result.IsSuccess)
                {
                    return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
                }
            }

            else if (user == "professor")
            {
                // Call the service method
                var result = await _professorService.RegisterProfessorAsync(model);

                // Check the result and return appropriate response
                if (!result.IsSuccess)
                {
                    return StatusCode(result.StatusCode, new { success = result.IsSuccess, message = result.ErrorMessage });
                }
            }

            else
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            return StatusCode(201, new { success = true, message = $"{user} registered successfully." });
        }


        [HttpPost("login/{user}")]
        public async Task<IActionResult> Login(string user, [FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            if(user == "student")
            {
                var result = await _studentService.LoginStudentAsync(model);

                if (!result.IsSuccess)
                {
                    return StatusCode(result.StatusCode, new { success = result.IsSuccess, message = result.ErrorMessage });
                }

                var (student, accessToken, refreshToken) = result.Data;

                _httpContextAccessor.HttpContext?.Response.Cookies.Append("accessToken", accessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddMinutes(15)
                });

                _httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                return Ok(new
                {
                    success = true,
                    student = student,
                    message = "Student logged in successfully."
                });
            }

            else if(user == "professor")
            {
                var result = await _professorService.LoginProfessorAsync(model);

                if (!result.IsSuccess)
                {
                    return StatusCode(result.StatusCode, new { success = result.IsSuccess, message = result.ErrorMessage });
                }

                var (professor, accessToken, refreshToken) = result.Data;

                _httpContextAccessor.HttpContext?.Response.Cookies.Append("accessToken", accessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddMinutes(15)
                });

                _httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                return Ok(new { success = true, professor = professor, message = $"{user} logged in successfully." });
            }

            return Ok();
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete("accessToken");
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete("refreshToken");

            return Ok(new
            {
                success = true,
                message = "User logged out successfully."
            });
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
