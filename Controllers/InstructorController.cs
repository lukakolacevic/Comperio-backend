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
//using dotInstrukcijeBackend.HelperFunctions;
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
    public class InstructorController : ControllerBase
    {
        private readonly IInstructorService _instructorService;
        private readonly IUserService _userService;
       
        public InstructorController(IInstructorService instructorService, IUserService userService)
        {
            _instructorService = instructorService;
            _userService = userService;
        }


        [Authorize]
        [HttpGet("professor/{email}")]

        public async Task<IActionResult> GetInstructorByEmail(string email)
        {
            var result = await _userService.FindUserByEmailAsync(2, email);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            return Ok(new { success = true, instructor = result.Data });
        }

        [Authorize]
        [HttpGet("instructors")]

        public async Task<IActionResult> GetTopFiveInstructorsByInstructionsCount()
        {
            var result = await _instructorService.FindTopFiveInstructorsBySessionCountAsync();

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            var listOfProfessors = result.Data;
            
            return Ok(new { success = true, instructors = listOfProfessors, message = "Top 5 instructors returned successfully!" });
        }

        
        [HttpDelete("instructor/{instructorId}/subjects/{subjectId}")]
        public async Task<IActionResult> RemoveInstructorFromSubject(int instructorId, int subjectId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            var professorIdToCheck = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (int.Parse(professorIdToCheck) != instructorId)
            {
                return Unauthorized(new { success = false, message = "Instructor unauthorized to remove subject." });
            }

            var result = await _instructorService.RemoveInstructorFromSubjectAsync(instructorId, subjectId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            return Ok(new { success = true, message = "Instructor removed from subject successfully." });
        }

        [Authorize(Roles = "2")]
        [HttpPost("instructor/{instructorId}/subjects/{subjectId}")]
        public async Task<IActionResult> JoinInstructorToSubject(int instructorId, int subjectId)
        {
            var professorIdToCheck = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (int.Parse(professorIdToCheck) != instructorId)
            {
                return Unauthorized(new { success = false, message = "Instructor unauthorized to join subject." });
            }

            var result = await _instructorService.JoinInstructorToSubject(instructorId, subjectId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            return Ok(new {success = true, message = "Instructor enrolled into subject successfully." });
        }
    }
}
