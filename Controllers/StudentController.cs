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
using dotInstrukcijeBackend.Interfaces;
using dotInstrukcijeBackend.ProfilePictureSavingUtility;
using dotInstrukcijeBackend.JWTTokenUtility;
using dotInstrukcijeBackend.DataTransferObjects;

namespace dotInstrukcijeBackend.Controllers
{
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository; // student repository

        private readonly IConfiguration _configuration;

        private readonly IWebHostEnvironment _hostingEnvironment;

        public StudentController(IStudentRepository studentRepository, IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _studentRepository = studentRepository;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }


        [HttpPost("register/student")]
        public async Task<IActionResult> Register([FromForm] StudentRegistrationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            if (await _studentRepository.GetStudentByEmailAsync(model.Email) is not null)
            {
                return BadRequest(new { success = false, message = "Email is already in use." });
            }

            //byte[] profilePictureSaved = await SaveProfilePicture(model.profilePicture); 

            var student = new Student
            {
                Email = model.Email,
                Name = model.Name,
                Surname = model.Surname,
                Password = PasswordHasher.HashPassword(model.Password),
                ProfilePicture = await ProfilePhotoSaver.SaveProfilePicture(model.ProfilePicture) 
            };

            await _studentRepository.AddStudentAsync(student);
            

            var response = new
            {
                success = true,
                message = "Student registered successfully!"
            };
            return StatusCode(201, response);
        }


        [HttpPost("login/student")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var student = await _studentRepository.GetStudentByEmailAsync(model.Email);

            if (student == null)
            {
                return BadRequest(new { success = false, message = "User not found." });
            }
            
            if (!PasswordHasher.VerifyPassword(student.Password, model.Password))
            {
                return BadRequest(new { success = false, message = "Invalid password." });
            }

            // Generiraj studentov JWT token 
            var token = JWTTokenGenerator.GenerateJwtToken(student, _configuration);

            String profilePhotoBase64String = student.ProfilePicture is not null ? Convert.ToBase64String(student.ProfilePicture) : null;

            return Ok(new
            {
                success = true,
                student = new StudentDTO(student.Id, student.Name, student.Surname, profilePhotoBase64String), 
                token,
                message = "Login successful."
            });
        }

        

        [Authorize]
        [HttpGet("student/{email}")]

        public async Task<IActionResult> GetStudentByEmail(string email)
        {
            var student = await _studentRepository.GetStudentByEmailAsync(email);

            if (student == null)
            {
                return NotFound(new { success = false, message = "Student not found." });
            }

            String profilePhotoBase64String = student.ProfilePicture is not null ? Convert.ToBase64String(student.ProfilePicture) : null;
            var studentDTO = new StudentDTO(student.Id, student.Name, student.Surname, profilePhotoBase64String);
            return Ok(new { success = true, student = studentDTO, message = "Student found successfully!" });
        }

        [Authorize]
        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            var listOfStudents = await _studentRepository.GetAllStudentsAsync();

            var listOfStudentsDTO = new List<StudentDTO>();

            foreach (var student in listOfStudents)
            {
                String profilePhotoBase64String = student.ProfilePicture is not null ? Convert.ToBase64String(student.ProfilePicture) : null;
                var studentDTO = new StudentDTO(student.Id, student.Name, student.Surname, profilePhotoBase64String);
                listOfStudentsDTO.Add(studentDTO);
            }

            return Ok(new {success = true, students = listOfStudentsDTO, message = "All students returned successfully!" });
        }


    }
}
