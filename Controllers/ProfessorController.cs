﻿using System;
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
       
        public ProfessorController(IProfessorService professorService)
        {
            _professorService = professorService;
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
