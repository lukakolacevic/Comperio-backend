//using dotInstrukcijeBackend.HelperFunctions;
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

        //[Authorize(Roles = "1")]
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

        //[Authorize(Roles = "1")]
        [HttpGet("sessions/students/{studentId}")]
        public async Task<IActionResult> GetAllStudentSessions(int studentId)
        {
            //var studentIdToCheck = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            if (studentId == 0)
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


        [Authorize(Roles = "2")]
        [HttpGet("sessions/instructors/{instructorId}")]
        public async Task<IActionResult> GetAllInstructorSessions(int instructorId)
        {
            var professorIdToCheck = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            if (instructorId != professorIdToCheck)
            {
                return Unauthorized(new { success = false, message = "Instructor unauthorized to get all sessions." });
            }
            var result = await _sessionService.GetAllInstructorSessionsAsync(instructorId);

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

        [Authorize(Roles = "2")]
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
