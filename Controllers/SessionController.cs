using dotInstrukcijeBackend.HelperFunctions;
using dotInstrukcijeBackend.Interfaces;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.Repositories;
using dotInstrukcijeBackend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace dotInstrukcijeBackend.Controllers
{
    public class SessionController : ControllerBase
    {
        private readonly ISessionRepository _sessionRepository;

        private readonly IStudentRepository _studentRepository;

        private readonly ILogger<SessionController> _logger;

        public SessionController(ISessionRepository sessionRepository, IStudentRepository studentRepository, ILogger<SessionController> logger)
        {
            _sessionRepository = sessionRepository;
            _studentRepository = studentRepository;
            _logger = logger;
        }

        [Authorize(Roles = "Student")]
        [HttpPost("sessions")]
        public async Task<IActionResult> ScheduleInstructionSession([FromBody] ScheduleSessionModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            var studentId = HttpContext.User.Claims.First(c => c.Type == "id").Value;

            //if (!await _studentRepository.CanScheduleMoreSessionsAsync(int.Parse(studentId)))
            //{
            // return BadRequest(new { success = false, message = "Maximum number of scheduled sessions reached." });
            //}
            

            var session = new Session
            {
                StudentId = int.Parse(studentId),
                ProfessorId = request.ProfessorId,
                SubjectId = request.SubjectId,
                DateTime = request.DateTime,
                Status = "Pending"
            };

            await _sessionRepository.AddSessionAysnc(session);

            return Ok(new { success = true, message = "Session created successfully." });
        }

        [Authorize(Roles = "Student")]
        [HttpGet("student/sessions")]
        public async Task<IActionResult> GetAllSessions()
        {
            var studentId = HttpContext.User.Claims.First(c => c.Type == "id").Value;

            var sessions = await _sessionRepository.GetAllStudentSessionsAsync(int.Parse(studentId));

            return Ok(new
            {
                success = true,

                pastSessions = sessions.PastSessions,

                upcomingSessions = sessions.UpcomingSessions,

                pendingRequests = sessions.PendingSessions,

                cancelledSessions = sessions.CancelledSessions,

                message = "All sessions retrieved successfully."
            });
        }

        [Authorize(Roles = "Professor")]
        [HttpGet("professor/sessions")]
        public async Task<IActionResult> GetAllProfessorSessions()
        {
            var professorId = int.Parse(HttpContext.User.Claims.First(c => c.Type == "id").Value);
            var sessions = await _sessionRepository.GetAllSessionsForProfessorAsync(professorId);

            return Ok(new
            {
                success = true,

                pastSessions = sessions.PastSessions,

                upcomingSessions = sessions.UpcomingSessions,

                pendingRequests = sessions.PendingSessions,

                cancelledSessions = sessions.CancelledSessions,

                message = "All professor sessions retrieved successfully."
            });
        }

        [Authorize(Roles = "Professor")]
        [HttpPut("sessions")]
        public async Task<IActionResult> ManageSessionRequest([FromBody] ManageSessionRequestModel request)
        {
            if (request == null || request.SessionId <= 0 || string.IsNullOrEmpty(request.NewStatus))
            {
                return BadRequest(new { success = false, message = "Invalid data provided." });
            }

            var professorId = HttpContext.User.Claims.First(c => c.Type == "id").Value;
            var session = await _sessionRepository.GetSessionByIdAsync(request.SessionId);

            if (session == null)
            {
                return NotFound(new { success = false, message = "Session not found." });
            }

            if (session.ProfessorId != int.Parse(professorId))
            {
                return Unauthorized(new { success = false, message = "Unauthorized professor." });
            }

            // Validate action type (accept or reject)
            if (request.NewStatus != "Confirmed" && request.NewStatus != "Cancelled")
            {
                return BadRequest(new { success = false, message = "Invalid action type." });
            }

            // Additional checks based on the action type
            if (request.NewStatus == "Confirmed" && session.Status != "Pending")
            {
                return BadRequest(new { success = false, message = "Only pending sessions can be accepted." });
            }

            // Perform the action (Accept or Reject)
            await _sessionRepository.ManageSessionRequestAsync(session.Id, request.NewStatus);

            return Ok(new
            {
                success = true,
                message = $"Session {request.NewStatus.ToLower()} successfully."
            });
        }

    }
}
