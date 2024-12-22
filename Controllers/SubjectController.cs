using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using dotInstrukcijeBackend.Data;
using dotInstrukcijeBackend.Models;
using Microsoft.AspNetCore.Authorization;
using dotInstrukcijeBackend.ViewModels;
using dotInstrukcijeBackend.DataTransferObjects;
//using dotInstrukcijeBackend.HelperFunctions;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Interfaces.Service;

namespace dotInstrukcijeBackend.Controllers
{
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService _subjectService;
        public SubjectController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }
        
        [Authorize(Roles = "2")]
        [HttpPost("subjects")]
        public async Task<IActionResult> CreateSubject([FromBody] SubjectRegistrationModel request)
        {
            //jedino profesorima treba dopustiti da stvaraju nove predmete
            if (!User.IsInRole("Professor"))
            {
                return Unauthorized("Unauthorized to create new subject"); 
            }
            var professorId = HttpContext.User.Claims.First(c => c.Type == "id").Value;

            var result = await _subjectService.CreateSubjectAsync(request, int.Parse(professorId));

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            return Ok(new { success = true, message = "Subject created successfully." });
            
        }

        [Authorize]
        [HttpGet("subject/{url}")]
        public async Task<IActionResult> GetSubjectByURL(string url)
        {
            var result = await _subjectService.FindSubjectByURLAsync(url);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            return Ok(new
            {
                success = true,
                subject = result.Data.subject,
                instructors = result.Data.instructors,
                message = "Subject and associated professors retrieved successfully."
            });
        }


        [HttpGet("subjects")]
        public async Task<IActionResult> GetAllSubjects()
        {
            var result = await _subjectService.FindAllSubjectsAsync();

            return Ok(new { success = true, subjects = result.Data });
        }

        [HttpGet("instructors/{instructorId}/subjects")]
        public async Task<IActionResult> GetAllSubjectsForInstructor(int instructorId)
        {
            var result = await _subjectService.FindAllSubjectsForInstructorAsync(instructorId);

            return Ok(new { success = true, subjects = result.Data });
        }
    }

}
