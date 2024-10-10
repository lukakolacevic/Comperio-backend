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
using dotInstrukcijeBackend.Interfaces;
using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.HelperFunctions;

namespace dotInstrukcijeBackend.Controllers
{
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectRepository _subjectRepository;

        private readonly IProfessorRepository _professorRepository; 

        public SubjectController(ISubjectRepository subjectRepository, IProfessorRepository professorRepository)
        {
            _subjectRepository = subjectRepository;
            _professorRepository = professorRepository;
        }
        
        [Authorize(Roles = "Professor")]
        [HttpPost("subject")]
        public async Task<IActionResult> CreateSubject([FromBody] SubjectRegistrationModel request)
        {
            //jedino profesorima treba dopustiti da stvaraju nove predmete
            if (!User.IsInRole("Professor"))
            {
                return Unauthorized("Unauthorized to create new subject"); 
            }
            // Provjera postoji li već predmet s istim naslovom ili kraticom
            var existingSubjectByTitle = await _subjectRepository.GetSubjectByTitleAsync(request.Title);
            if (existingSubjectByTitle != null)
            {
                return BadRequest(new { success = false, message = "Subject with given title already exists." });
            }

            var existingSubjectDetailsByURL = await _subjectRepository.GetSubjectByURLAsync(request.Url);
            var existingSubjectByURL = existingSubjectDetailsByURL.Subject;
            if (existingSubjectByURL != null)
            {
                return BadRequest(new { success = false, message = "Subject with given URL already exists." });
            }

            var subject = new Subject
            {
                Title = request.Title,
                Url = request.Url,
                Description = request.Description
            };

            var subjectId = await _subjectRepository.AddSubjectAsync(subject); //mijenjaj ovo da mozes koristiti ovo u liniji 62
            var professorId = HttpContext.User.Claims.First(c => c.Type == "id").Value;
            
            await _professorRepository.AssociateProfessorWithSubjectAsync(int.Parse(professorId), subjectId);
            
            return Ok(new { success = true, message = "Subject created successfully." });
        }

        [Authorize]
        [HttpGet("subject/{url}")]
        public async Task<IActionResult> GetSubjectByURL(string url)
        {
            var subjectDetails = await _subjectRepository.GetSubjectByURLAsync(url);

            if (subjectDetails.Subject is null) 
            {
                return BadRequest(new {success = false, message = "Subject not found."});
            }


            var subject = new Subject
            {
                Id = subjectDetails.Subject.Id,
                Title = subjectDetails.Subject.Title,
                Url = subjectDetails.Subject.Url,
                Description = subjectDetails.Subject.Description  //Vrati ovo frontendu
            };

            var listOfProfessors = subjectDetails.SubjectProfessors;

            var listOfProfessorsDTO = new List<ProfessorDTO>();

            foreach (var professor in listOfProfessors)
            {
                var professorDTO = MapToProfessorDTO.mapToProfessorDTO(professor);
                listOfProfessorsDTO.Add(professorDTO);
            }

            return Ok(new
            {
                success = true,
                subject = subject,
                professors = listOfProfessorsDTO,
                message = "Subject and associated professors retrieved successfully"
            });
        }

        
        [HttpGet("subjects")]
        public async Task<IActionResult> GetAllSubjects()
        {
            var subjects = await _subjectRepository.GetAllSubjectsAsnyc();

            return Ok(new
            {
                success = true,
                subjects = subjects
            });
        }

        [HttpGet("professors/{professorId}/subjects")]
        public async Task<IActionResult> GetAllSubjectsForProfessor(int professorId)
        {
            var subjects = await _subjectRepository.GetAllSubjectsForProfessorAsync(professorId);

            return Ok(new
            {
                success = true,
                subjects = subjects
            });
        }
        

        
    }

}
