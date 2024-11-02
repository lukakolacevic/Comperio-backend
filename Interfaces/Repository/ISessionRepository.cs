using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Models;

namespace dotInstrukcijeBackend.Interfaces.RepositoryInterfaces
{
    public interface ISessionRepository
    {
        Task AddSessionAysnc(Session session);

        Task<SessionsDTO<SessionWithProfessorDTO>> GetAllStudentSessionsAsync(int studentId);

        Task<SessionsDTO<SessionWithStudentDTO>> GetAllSessionsForProfessorAsync(int professorId);

        Task ManageSessionRequestAsync(int sessionId, string newStatus);

        Task<Session> GetSessionByIdAsync(int sessionId);
    }
}
