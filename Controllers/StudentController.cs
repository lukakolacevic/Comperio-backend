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
using NServiceBus.Testing;
using Microsoft.AspNetCore.Hosting;
using dotInstrukcijeBackend.Repositories;
using dotInstrukcijeBackend.ProfilePictureSavingUtility;
using dotInstrukcijeBackend.JWTTokenUtility;
using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Interfaces.ServiceInterfaces;
using Azure.Core;

namespace dotInstrukcijeBackend.Controllers
{
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        

        public StudentController(IStudentService studentService)
        { 
            _studentService = studentService;
        }


        [Authorize]
        [HttpGet("student/{email}")]
        public async Task<IActionResult> GetStudentByEmail(string email)
        {
            var result = await _studentService.FindStudentByEmailAsync(email);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            var student = result.Data;
            return Ok(new { success = true, student = student, message = "Student returned successfuly." });
        }


        [Authorize]
        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            var result = await _studentService.FindAllStudentsAsync();

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }
            
            var students = result.Data;

            return Ok(new
            {
                success = true,
                students = students,
                message = "All students returned successfully."
            });

        }   

        [Authorize(Roles = "Student")]
        [HttpGet("students/{studentId}/stats/popular-subjects")]
        public async Task<IActionResult> GetTopFiveRequestedSubjects(int studentId)
        {
            var studentIdToCheck = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            if (studentId != studentIdToCheck)
            {
                return Unauthorized(new { success = false, message = "Student unauthorized to get most popular subjects." });
            }

            var result = await _studentService.FindTopFiveRequestedSubjectsAsync(studentId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            var listOfMostChosenSubjects = result.Data;
            return Ok(new
            {
                success = true,
                listOfMostChosenSubjects = listOfMostChosenSubjects,
                message = "Top subjects returned successfully."
            });
        }

        [Authorize(Roles = "Student")]
        [HttpGet("students/{studentId}/stats/popular-professors")]
        public async Task<IActionResult> GetTopFiveRequestedProfessors(int studentId)
        {
            var studentIdToCheck = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            if (studentId != studentIdToCheck)
            {
                return Unauthorized(new { success = false, message = "Student unauthorized to get most popular subjects." });
            }

            var result = await _studentService.FindTopFiveRequestedProfessorsAsync(studentId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            var listOfMostChosenProfessors = result.Data;

            return Ok(new
            {
                success = true,
                listOfMostChosenProfessors = listOfMostChosenProfessors,
                message = "Top professors returned successfully."
            });
        }
    }
}
