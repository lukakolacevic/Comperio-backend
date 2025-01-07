using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.Interfaces.RepositoryInterfaces
{
    public interface ISessionRepository
    {
        Task AddSessionAysnc(Session session);

        Task<SessionsDTO<SessionWithUserDTO>> GetAllStudentSessionsAsync(int studentId);

        Task<SessionsDTO<SessionWithUserDTO>> GetAllInstructorSessionsAsync(int instructorId);

        Task ManageSessionRequestAsync(int sessionId, string newStatus);

        Task<Session> GetSessionByIdAsync(int sessionId);
        Task<SessionDetailsDTO> GetSessionDetailsAsync(int sessionId);
    }
}
