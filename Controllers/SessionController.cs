using dotInstrukcijeBackend.HelperFunctions;
using dotInstrukcijeBackend.Interfaces.RepositoryInterfaces;
using dotInstrukcijeBackend.Interfaces.Service;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.Repositories;
using dotInstrukcijeBackend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace dotInstrukcijeBackend.Controllers
{
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;

        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [Authorize(Roles = "Student")]
        [HttpPost("sessions")]
        public async Task<IActionResult> ScheduleInstructionSession([FromBody] ScheduleSessionModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided.", code = "INVALID_CREDENTIALS" });
            }

            var studentId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            var result = await _sessionService.ScheduleSessionAsync(studentId, request);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage});
            }

            return Ok(new { success = true, message = "Session created successfully." });
        }

        [Authorize(Roles = "Student")]
        [HttpGet("sessions/students/{studentId}")]
        public async Task<IActionResult> GetAllStudentSessions(int studentId)
        {
            var studentIdToCheck = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            if (studentId != studentIdToCheck)
            {
                return Unauthorized(new { success = false, message = "Student unauthorized to get all sessions." });
            }
            var result = await _sessionService.GetAllStudentSessionsAsync(studentId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            return Ok(new
            {
                success = true,
                pastSessions = result.Data.PastSessions,
                upcomingSessions = result.Data.UpcomingSessions,
                pendingRequests = result.Data.PendingSessions,
                cancelledSessions = result.Data.CancelledSessions,
                message = "All student sessions retrieved successfully."
            });
        }


        [Authorize(Roles = "Professor")]
        [HttpGet("sessions/professors/{professorId}")]
        public async Task<IActionResult> GetAllProfessorSessions(int professorId)
        {
            var professorIdToCheck = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            if (professorId != professorIdToCheck)
            {
                return Unauthorized(new { success = false, message = "Professor unauthorized to get all sessions." });
            }
            var result = await _sessionService.GetAllProfessorSessionsAsync(professorId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            return Ok(new
            {
                success = true,
                pastSessions = result.Data.PastSessions,
                upcomingSessions = result.Data.UpcomingSessions,
                pendingRequests = result.Data.PendingSessions,
                cancelledSessions = result.Data.CancelledSessions,
                message = "All professor sessions retrieved successfully."
            });
        }

        [Authorize(Roles = "Professor")]
        [HttpPatch("sessions")]
        public async Task<IActionResult> ManageSessionRequest([FromBody] ManageSessionRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            var professorId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            var result = await _sessionService.ManageSessionRequestAsync(professorId, request);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { success = false, message = result.ErrorMessage });
            }

            return Ok(new { success = true, message = $"Session {request.NewStatus.ToLower()} successfully." });
        }

    }
}
