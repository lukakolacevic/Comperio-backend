using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using dotInstrukcijeBackend.Data;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.ViewModels;
using dotInstrukcijeBackend.PasswordHashingUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using dotInstrukcijeBackend.ProfilePictureSavingUtility;
using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.HelperFunctions;
using System.CodeDom;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Interfaces.Service;
using dotInstrukcijeBackend.Services;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis;
using Azure.Core;

namespace dotInstrukcijeBackend.Controllers
{
    [ApiController]
    public class ProfessorController : ControllerBase
    {
        private readonly IProfessorService _professorService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ProfessorController(IProfessorService professorService, IHttpContextAccessor httpContextAccessor)
        {
            _professorService = professorService;
            _httpContextAccessor = httpContextAccessor;

        }


        [HttpPost("register/professor")]
        public async Task<IActionResult> Register([FromForm] ProfessorRegistrationModel model)
        {
            var result = await _professorService.RegisterProfessorAsync(model);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            return Ok(new { success = true, message = "Professor registered successfully." });
        }


        [HttpPost("login/professor")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var result = await _professorService.LoginProfessorAsync(model);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            var (professor, accessToken, refreshToken) = result.Data;

            Console.WriteLine(accessToken);
            Console.WriteLine(refreshToken);

            if (_httpContextAccessor.HttpContext == null)
            {
                Console.WriteLine("check");
            }

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

            return Ok(new { success = true, professor = professor, message = "Professor logged in successfully." });
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



        [Authorize]
        [HttpGet("professor/{email}")]

        public async Task<IActionResult> GetProfessorByEmail(string email)
        {
            var result = await _professorService.FindProfessorByEmailAsync(email);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            return Ok(new { success = true, professor = result.Data });
        }

        [Authorize]
        [HttpGet("professors")]

        public async Task<IActionResult> GetTopFiveProfessorsByInstructionsCount()
        {
            var result = await _professorService.FindTopFiveProfessorsByInstructionsCountAsync();

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            var listOfProfessors = result.Data;
            
            return Ok(new { success = true, professors = listOfProfessors, message = "Top 5 professors returned successfully!" });
        }

        [Authorize]
        [HttpDelete("professors/{professorId}/subjects/{subjectId}")]
        public async Task<IActionResult> RemoveProfessorFromSubject(int professorId, int subjectId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            var professorIdToCheck = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (int.Parse(professorIdToCheck) != professorId)
            {
                return Unauthorized(new { success = false, message = "Professor unauthorized to remove subject." });
            }

            var result = await _professorService.RemoveProfessorFromSubjectAsync(professorId, subjectId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            return Ok(new { success = true, message = "Professor removed from subject successfully." });
        }

        [Authorize(Roles = "Professor")]
        [HttpPost("professor/{professorId}/subjects/{subjectId}")]
        public async Task<IActionResult> JoinProfessorToSubject(int professorId, int subjectId)
        {
            var professorIdToCheck = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (int.Parse(professorIdToCheck) != professorId)
            {
                return Unauthorized(new { success = false, message = "Professor unauthorized to join subject." });
            }

            var result = await _professorService.JoinProfessorToSubject(professorId, subjectId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            return Ok(new {success = true, message = "Professor enrolled into subject successfully."});
        }
    }
}
