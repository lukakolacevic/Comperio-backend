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
using dotInstrukcijeBackend.Interfaces;
using dotInstrukcijeBackend.ProfilePictureSavingUtility;
using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.HelperFunctions;
using System.CodeDom;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;

namespace dotInstrukcijeBackend.Controllers
{
    [ApiController]
    public class ProfessorController : ControllerBase
    {
        private readonly ISubjectRepository _subjectRepository; 

        private readonly IProfessorRepository _professorRepository;

        private readonly ISessionRepository _sessionRepository; 

        private readonly IConfiguration configuration;

        public ProfessorController(IProfessorRepository professorRepository, ISubjectRepository subjectRepository, ISessionRepository sessionRepository, IConfiguration configuration)
        {
            _professorRepository = professorRepository;
            _subjectRepository = subjectRepository;
            _sessionRepository = sessionRepository;
            this.configuration = configuration;
        }

        [HttpPost("register/professor")]
        public async Task<IActionResult> Register([FromForm] ProfessorRegistrationModel request)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            if (await _professorRepository.GetProfessorByEmailAsync(request.Email) is not null)
            {
                return BadRequest(new { success = false, message = "Email is already in use." });
            }

            var professor = new Professor
            {
                Email = request.Email,
                Name = request.Name,
                Surname = request.Surname,
                Password = PasswordHasher.HashPassword(request.Password),
                //subjects = model.subjects,
                ProfilePicture = await ProfilePhotoSaver.SaveProfilePicture(request.ProfilePicture),
                InstructionsCount = 0     //postavi broj instrukcija na nulu kad se stvori novi profesor
            };

            await _professorRepository.AddProfessorAsync(professor); 

            var prof = await _professorRepository.GetProfessorByEmailAsync(professor.Email);
            int profId = prof.Id;

            if(request.Subjects is not null)
            {
                foreach (var subjectURL in request.Subjects)
                {
                    var subjectDetails = await _subjectRepository.GetSubjectByURLAsync(subjectURL);

                    int subjectId = subjectDetails.Subject.Id;

                    await _professorRepository.AssociateProfessorWithSubjectAsync(profId, subjectId);
                }
            }
            //await context.SaveChangesAsync();

            var response = new
            {
                success = true,
                message = "Professor registered successfully!"
            };
            return StatusCode(201, response);
        }

        private string GenerateJwtToken(Professor professor)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, professor.Email),
                    new Claim("id", professor.Id.ToString()),
                    new Claim(ClaimTypes.Name, professor.Name + " " + professor.Surname),
                    new Claim(ClaimTypes.Role, "Professor")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPost("login/professor")]
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            var professor = await _professorRepository.GetProfessorByEmailAsync(request.Email);

            if (professor == null)
            {
                return BadRequest(new { success = false, message = "Professor not found." });
            }

            if (!PasswordHasher.VerifyPassword(professor.Password, request.Password))
            {
                return BadRequest(new { success = false, message = "Invalid password." });
            }

            // Generiraj profesorov JWT token 
            var token = GenerateJwtToken(professor);
            

            return Ok(new
            {
                success = true,
                professor = MapToProfessorDTO.mapToProfessorDTO(professor), 
                token = token,
                message = "Login successful."
            });
        }

        [Authorize]
        [HttpGet("professor/{email}")]

        public async Task<IActionResult> GetProfessorByEmail(string email)
        {
            var professor = await _professorRepository.GetProfessorByEmailAsync(email);

            if (professor == null)
            {
                return NotFound(new { success = false, message = "Professor not found." });
            }
            var professorDTO = MapToProfessorDTO.mapToProfessorDTO(professor);

            return Ok(new { success = true, professor = professorDTO, message = "Professor found successfully!" });
        }

        [Authorize]
        [HttpGet("professors")]

        public async Task<IActionResult> GetTopFiveProfessorsByInstructionsCount()
        {
            var listOfProfessors = await _professorRepository.GetTopFiveProfessorsByInstructionsCountAsync();

            var listOfProfessorsDTO = new List<ProfessorDTO>();

            foreach (var professor in listOfProfessors)
            {
                var professorDTO = MapToProfessorDTO.mapToProfessorDTO(professor);
                listOfProfessorsDTO.Add(professorDTO);
            }

            return Ok(new { success = true, professors = listOfProfessorsDTO, message = "Top 5 professors returned successfully!" });
        }

        [Authorize]
        [HttpDelete("professor-subjects")]
        public async Task<IActionResult> RemoveProfessorFromSubject([FromBody] RemoveProfessorFromSubjectModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            int professorToRemoveId = request.ProfessorId;
            int subjectId = request.SubjectId;

            var professor = _professorRepository.GetProfessorByIdAsync(professorToRemoveId);
            if (professor == null)
            {
                return NotFound(new { success = false, message = "Professor not found." });
            }

            var subject = _subjectRepository.GetSubjectByIdAsync(subjectId);
            if (subject == null)
            {
                return NotFound(new { success = false, message = "Subject not found." });
            }

            await _professorRepository.RemoveProfessorFromSubjectAsync(professorToRemoveId, subjectId);

            return Ok(new { success = true, mesage = "Professor removed from subject successfully." });
        }

        [Authorize(Roles = "Professor")]
        [HttpPost("professor/{professorId}/subjects/{subjectId}/join")]
        public async Task<IActionResult> JoinProfessorToSubject(int professorId, int subjectId)
        {
            var professor = await _professorRepository.GetProfessorByIdAsync(professorId);
            if (professor == null)
            {
                return BadRequest(new { success = false, message = "Professor not found.", code = "PROFESSOR_DOES_NOT_EXIST" });
            }

            var subject = await _subjectRepository.GetSubjectByIdAsync(subjectId);
            if (subject == null)
            {
                return BadRequest(new { success = false, message = "Subject not found.", code = "SUBJECT_DOES_NOT_EXIST" });
            }

            if (await _professorRepository.IsProfessorTeachingSubject(professorId, subjectId))
            {
                return BadRequest(new { success = false, message = "The professor is already teaching this subject.", code = "PROFESSOR_ALREADY_TEACHING_SUBJECT" });
            }

            await _professorRepository.AssociateProfessorWithSubjectAsync(professorId, subjectId);

            return Ok(new {success = true, message = "Professor enrolled into subject successfully."});
        }
    }
}
