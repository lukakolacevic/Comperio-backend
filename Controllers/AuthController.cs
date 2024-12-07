using Azure.Core;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Interfaces.Service;
using dotInstrukcijeBackend.Interfaces.ServiceInterfaces;
using dotInstrukcijeBackend.Interfaces.User;
using dotInstrukcijeBackend.Interfaces.Utility;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.Services;
using dotInstrukcijeBackend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using NuGet.Versioning;
using SendGrid.Helpers.Mail.Model;

namespace dotInstrukcijeBackend.Controllers
{
    
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly IProfessorService _professorService;
        private readonly IStudentService _studentService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenService _tokenService;
       
        public AuthController(ITokenService tokenService, IProfessorService professorService, IStudentService studentService, IHttpContextAccessor httpContextAccessor, EmailService emailService)
        {
            _emailService = emailService;
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

            IUser userToRegister = null; 
            if (user == "student")
            {
                var result = await _studentService.RegisterStudentAsync(model);
                
                if (!result.IsSuccess)
                {
                    return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
                }

                var student = _studentService.FindStudentByEmailAsync(model.Email);

                var studentId = student.Id;
                var token = _tokenService.GenerateEmailVerificationToken(studentId);

                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { token, email = model.Email }, Request.Scheme);
                await _emailService.SendEmailAsync(model.Email, "Potvrda email adrese", confirmationLink);
            }

            else if (user == "professor")
            {
                var result = await _professorService.RegisterProfessorAsync(model);
                
                if (!result.IsSuccess)
                {
                    return StatusCode(result.StatusCode, new { success = result.IsSuccess, message = result.ErrorMessage });
                }

                var professor = _professorService.FindProfessorByEmailAsync(model.Email);
                var professorId = professor.Id;

                var token = _tokenService.GenerateEmailVerificationToken(professorId);

                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { token, email = model.Email }, Request.Scheme);
                await _emailService.SendEmailAsync(model.Email, "Potvrda email adrese", confirmationLink);
            }

            else
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            //var token = _tokenService.GenerateEmailVerificationToken(student.StudentId);
            //var confirmationLink = Url.Action(nameof("confirm-email"), "Auth", new {token, email = student.})

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

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var student_result = await _studentService.FindStudentByEmailAsync(email);
            var professor_result = await _professorService.FindProfessorByEmailAsync(email); //dovrsi ovo, zovi metodu za token gore, nek se posalje mail

            if (!student_result.IsSuccess && !professor_result.IsSuccess)
            {
                return BadRequest(new { success = false, message = "User not found." });
            }

            if (student_result.IsSuccess)
            {
                await _studentService.ConfirmEmailAsync(student_result.Data.StudentId);
                return Redirect("http://localhost:5173/confirm-email-success");
            }

            if (professor_result.IsSuccess)
            {   
                await _professorService.ConfirmEmailAsync(professor_result.Data.ProfessorId);
                return Redirect("http://localhost:5173/confirm-email-success");
            }

            return BadRequest(new { success = false, message = "Could not confirm user email." });
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] string email)
        { 
            var student = await _studentService.FindStudentByEmailAsync(email);
            var professor = await _professorService.FindProfessorByEmailAsync(email);
            var token = student == null ? _tokenService.GenerateEmailVerificationToken(professor.Data.ProfessorId) : _tokenService.GenerateEmailVerificationToken(student.Data.StudentId);

            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { token, email = email }, Request.Scheme);

            if (student != null)
            {
                await _emailService.SendEmailAsync(professor.Data.Email, "Potvrda email adrese", confirmationLink);
                return Ok(new { success = true, message = "Confirmation email resent successfully." });
            }
            else if (professor != null)
            {
                await _emailService.SendEmailAsync(email, "Potvrda email adrese", confirmationLink);
                return Ok(new { success = true, message = "Confirmation email resent successfully." });
            }

            return BadRequest(new { success = false, message = "User not found." });
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
    }
}
