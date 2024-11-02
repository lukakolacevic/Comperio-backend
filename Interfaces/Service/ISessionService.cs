using dotInstrukcijeBackend.DataTransferObjects;
using dotInstrukcijeBackend.Models;
using dotInstrukcijeBackend.ServiceResultUtility;
using dotInstrukcijeBackend.ViewModels;

namespace dotInstrukcijeBackend.Interfaces.Service
{
    public interface ISessionService
    {
        Task<ServiceResult> ScheduleSessionAsync(int studentId, ScheduleSessionModel request);
        Task<ServiceResult<SessionsDTO<SessionWithProfessorDTO>>> GetAllStudentSessionsAsync(int studentId);
        Task<ServiceResult<SessionsDTO<SessionWithStudentDTO>>> GetAllProfessorSessionsAsync(int professorId);
        Task<ServiceResult> ManageSessionRequestAsync(int professorId, ManageSessionRequestModel request);
    }
}
