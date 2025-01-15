using Azure.Core;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Interfaces.Service;
using dotInstrukcijeBackend.Interfaces.ServiceInterfaces;
//using dotInstrukcijeBackend.Interfaces.User;
using dotInstrukcijeBackend.Interfaces.Utility;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.ServiceResultUtility;
using dotInstrukcijeBackend.Services;
using dotInstrukcijeBackend.ViewModels;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using NuGet.Versioning;
using SendGrid.Helpers.Mail.Model;
using System.Data;

namespace dotInstrukcijeBackend.Controllers
{
    
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly IInstructorService _professorService;
        private readonly IStudentService _studentService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
       
        public AuthController(ITokenService tokenService, IInstructorService professorService, IStudentService studentService, IHttpContextAccessor httpContextAccessor, EmailService emailService, IUserService userService)
        {
            _emailService = emailService;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
            _professorService = professorService;
            _studentService = studentService;
            _userService = userService;
        }

        [HttpPost("register/{roleId}")]
        public async Task<IActionResult> Register(int roleId, [FromForm] RegistrationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            if(roleId != 1 && roleId != 2)
            {
                return BadRequest(new { success = false, message = "Invalid roleId provided." });
            }

            var registrationResult = await _userService.RegisterUserAsync(roleId, model);
            if (!registrationResult.IsSuccess)
            {
                return BadRequest(new { success = false, message = registrationResult.ErrorMessage });
            }

            var user = await _userService.FindUserByEmailAsync(roleId, model.Email);
            var userId = user.Data.Id;

            var token = _tokenService.GenerateEmailVerificationToken(userId);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { token, email = model.Email, id = userId }, Request.Scheme);
            await _emailService.SendEmailAsync(model.Email, "Potvrda email adrese", confirmationLink);
     
            return StatusCode(201, new { success = true, message = $"User registered successfully." });
        }


        [HttpPost("login/{roleId}")]
        public async Task<IActionResult> Login(int roleId, [FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            var result = await _userService.LoginUserAsync(roleId, model);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            var (user, accessToken, refreshToken) = result.Data;
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
                user = user,
                message = "User logged in successfully."
            });
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

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token, int id)
        {
            var userResult = await _userService.FindUserByIdAsync(id);
            if (!userResult.IsSuccess)
            {
                return BadRequest(new { success = false, message = "User not found." });
            }

            await _userService.ConfirmEmailAsync(userResult.Data.Id);
            return Redirect("http://localhost:5173/confirm-email-success");
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] string email)
        {
            
            var studentResult = await _userService.FindUserByEmailAsync(1, email);
            var professorResult = await _userService.FindUserByEmailAsync(2, email);
            ServiceResult<User> userResult = studentResult.IsSuccess ? studentResult : professorResult;

            if (!userResult.IsSuccess)
            {
                return BadRequest(new { success = false, message = "User not found." });
            }
            var user = userResult.Data;
            var token = _tokenService.GenerateEmailVerificationToken(user.Id);

            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { token, email = email, id = user.Id }, Request.Scheme);

            await _emailService.SendEmailAsync(user.Email, "Potvrda email adrese", confirmationLink);

            return Ok(new { success = true, message = $"Confirmation email resent successfully." });
        }

        [HttpGet("refresh-token")]
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
                Secure = false,                 
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(15) 
            });

            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,                 
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7) 
            });

            return Ok(new
            {
                success = true,
                message = "Token refreshed successfully."
            });
        }

        [HttpPost("google-login/{roleId}")]
        public async Task<IActionResult> GoogleLogin(int roleId, [FromBody] TokenRequest request)
        {
            try
            {
                Console.WriteLine(request.Token);
                // Validate Google token
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token);
                Console.WriteLine("Tu sam.");
                if (payload == null || string.IsNullOrEmpty(payload.Email))
                {
                    Console.WriteLine("Tu sam.");
                    return BadRequest(new { success = false, message = "Invalid Google token." });
                }

                // Check if the user already exists in the database
                var userResult = await _userService.FindUserByEmailAsync(roleId, payload.Email);
                User user;

                if (!userResult.IsSuccess)
                {
                    // If user doesn't exist, register them
                    var newUser = new User
                    {
                        RoleId = roleId,
                        Name = payload.GivenName,
                        Surname = payload.FamilyName,
                        Email = payload.Email,
                        PasswordHash = null,
                        ProfilePicture = payload.Picture,
                        OAuthId = payload.Subject,
                        CreatedAt = DateTime.UtcNow,
                        IsVerified = true

                        // Passed from the frontend: e.g., 1 = student, 2 = instructor
            
                    };

                    var registrationResult = await _userService.RegisterGoogleUserAsync(roleId, newUser);
                    if (!registrationResult.IsSuccess)
                    {
                        return StatusCode(500, new { success = false, message = "Failed to register user." });
                    }

                    user = registrationResult.Data;
                }
                else
                {
                    // If user exists, retrieve them
                    user = userResult.Data;
                }

                // Generate tokens
                var accessToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken(user);

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

                // Return success response
                return Ok(new
                {
                    success = true,
                    user = new { user.Id, user.Email, user.Name, user.Surname, user.RoleId },
                    
                });

            }
            catch (InvalidJwtException)
            {
                return Unauthorized(new { success = false, message = "Invalid Google token." });
            }
        }

    }
}
